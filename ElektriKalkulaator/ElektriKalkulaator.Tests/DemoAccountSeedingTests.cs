using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElektriKalkulaator.Tests
{
    // Tests for the account seeder, and above all for its production safety guard.
    //
    // The demo accounts have passwords written in plain text in the source code, which is fine for
    // local development and catastrophic anywhere real — anyone who reads the repository would know
    // the administrator password. The ONLY thing preventing that is the isDevelopment flag.
    //
    // A safety mechanism nobody has tested is a safety mechanism nobody should trust, so the most
    // important test here is the one asserting the accounts are NOT created outside development.
    public class DemoAccountSeedingTests
    {
        // Builds an isolated Identity stack over an in-memory database, so each test seeds into a
        // clean slate and cannot see another test's users.
        private static ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddDbContext<ElektriKalkulaatorContext>(options =>
            {
                options.UseInMemoryDatabase($"SeederTest_{Guid.NewGuid()}");
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    // Mirror the real rules from Program.cs, otherwise this test could pass with a
                    // password the actual application would reject.
                    options.Password.RequiredLength         = 8;
                    options.Password.RequireDigit           = true;
                    options.Password.RequireLowercase       = true;
                    options.Password.RequireUppercase       = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.User.RequireUniqueEmail         = true;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ElektriKalkulaatorContext>();

            return services.BuildServiceProvider();
        }

        private static IConfiguration EmptyConfiguration() =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

        private static IConfiguration ConfigurationWithAdmin(string email, string password) =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminUser:Email"] = email,
                ["AdminUser:Password"] = password
            }).Build();

        // ── THE SAFETY GUARD ────────────────────────────────────────────────────

        [Fact]
        public async Task DemoAccounts_AreNotCreated_OutsideDevelopment()
        {
            using var provider = BuildServices();

            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: false);

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            Assert.Null(await userManager.FindByEmailAsync(IdentitySeeder.DemoAdminEmail));
            Assert.Null(await userManager.FindByEmailAsync(IdentitySeeder.DemoCustomerEmail));
        }

        [Fact]
        public async Task DemoAccounts_AreCreated_InDevelopment()
        {
            using var provider = BuildServices();

            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            Assert.NotNull(await userManager.FindByEmailAsync(IdentitySeeder.DemoAdminEmail));
            Assert.NotNull(await userManager.FindByEmailAsync(IdentitySeeder.DemoCustomerEmail));
        }

        // ── ROLES ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task BothRoles_AlwaysExist_EvenWithNoAccountsConfigured()
        {
            using var provider = BuildServices();

            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: false);

            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            Assert.True(await roleManager.RoleExistsAsync(UserRoles.Admin));
            Assert.True(await roleManager.RoleExistsAsync(UserRoles.Customer));
        }

        [Fact]
        public async Task DemoAdmin_IsInTheAdminRole()
        {
            using var provider = BuildServices();
            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userManager.FindByEmailAsync(IdentitySeeder.DemoAdminEmail);

            Assert.True(await userManager.IsInRoleAsync(admin!, UserRoles.Admin));
            Assert.False(await userManager.IsInRoleAsync(admin!, UserRoles.Customer));
        }

        [Fact]
        public async Task DemoCustomer_IsInTheCustomerRole_AndNotAnAdmin()
        {
            // The point of the customer account is testing what a NON-admin sees. If it were
            // accidentally given the Admin role the account would be useless for that.
            using var provider = BuildServices();
            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var customer = await userManager.FindByEmailAsync(IdentitySeeder.DemoCustomerEmail);

            Assert.True(await userManager.IsInRoleAsync(customer!, UserRoles.Customer));
            Assert.False(await userManager.IsInRoleAsync(customer!, UserRoles.Admin));
        }

        // ── PASSWORDS ───────────────────────────────────────────────────────────

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DemoPasswords_ActuallyWork(bool checkAdmin)
        {
            // Guards against the documented password drifting away from the seeded one — the
            // failure mode being someone following the docs and being unable to sign in.
            using var provider = BuildServices();
            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            var email    = checkAdmin ? IdentitySeeder.DemoAdminEmail    : IdentitySeeder.DemoCustomerEmail;
            var password = checkAdmin ? IdentitySeeder.DemoAdminPassword : IdentitySeeder.DemoCustomerPassword;

            var user = await userManager.FindByEmailAsync(email);

            Assert.NotNull(user);
            Assert.True(await userManager.CheckPasswordAsync(user!, password),
                $"The seeded password for {email} does not match the documented one.");
        }

        [Fact]
        public async Task PasswordsAreStoredHashed_NeverAsPlainText()
        {
            using var provider = BuildServices();
            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(IdentitySeeder.DemoAdminEmail);

            Assert.NotNull(user!.PasswordHash);
            Assert.DoesNotContain(IdentitySeeder.DemoAdminPassword, user.PasswordHash);
        }

        // ── REPEATED RUNS ───────────────────────────────────────────────────────

        [Fact]
        public async Task RunningTheSeederTwice_DoesNotDuplicateAnyone()
        {
            // The seeder runs on every application start, so this is the normal case rather than
            // an edge case.
            using var provider = BuildServices();

            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);
            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);

            var context = provider.GetRequiredService<ElektriKalkulaatorContext>();

            Assert.Equal(1, context.Users.Count(u => u.Email == IdentitySeeder.DemoAdminEmail));
            Assert.Equal(1, context.Users.Count(u => u.Email == IdentitySeeder.DemoCustomerEmail));
        }

        [Fact]
        public async Task AChangedPassword_IsNotResetByRestarting()
        {
            // Someone may change a demo password while testing. Re-seeding must leave it alone
            // rather than silently putting the original back.
            using var provider = BuildServices();
            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(IdentitySeeder.DemoCustomerEmail);

            // ChangePasswordAsync rather than a reset token: a reset needs token providers
            // registered, which this deliberately minimal test setup does not have. Changing with
            // the current password exercises the same thing without that dependency.
            var change = await userManager.ChangePasswordAsync(
                user!, IdentitySeeder.DemoCustomerPassword, "Muudetud456");
            Assert.True(change.Succeeded,
                "Could not change the password: " + string.Join("; ", change.Errors.Select(e => e.Description)));

            await IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: true);

            var after = await userManager.FindByEmailAsync(IdentitySeeder.DemoCustomerEmail);
            Assert.True(await userManager.CheckPasswordAsync(after!, "Muudetud456"));
        }

        // ── CONFIGURED ADMIN ────────────────────────────────────────────────────

        [Fact]
        public async Task ConfiguredAdmin_IsCreatedInAnyEnvironment()
        {
            // The real admin comes from User Secrets and must work in production, unlike the demos.
            using var provider = BuildServices();

            await IdentitySeeder.SeedAsync(
                provider, ConfigurationWithAdmin("real@example.com", "RealPass123"), isDevelopment: false);

            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userManager.FindByEmailAsync("real@example.com");

            Assert.NotNull(admin);
            Assert.True(await userManager.IsInRoleAsync(admin!, UserRoles.Admin));
        }

        [Fact]
        public async Task MissingAdminConfiguration_DoesNotCrashTheApplication()
        {
            // Starting with no admin configured should log a warning and carry on, not throw —
            // otherwise a fresh clone would fail to start at all.
            using var provider = BuildServices();

            var exception = await Record.ExceptionAsync(() =>
                IdentitySeeder.SeedAsync(provider, EmptyConfiguration(), isDevelopment: false));

            Assert.Null(exception);
        }
    }
}
