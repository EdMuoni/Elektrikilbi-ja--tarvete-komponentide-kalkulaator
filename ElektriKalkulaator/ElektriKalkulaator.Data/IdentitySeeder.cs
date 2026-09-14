using ElektriKalkulaator.Core.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElektriKalkulaator.Data
{
    // Creates the two roles and the first admin account when the application starts.
    //
    // Roles and users cannot be seeded the usual way (modelBuilder.HasData) because passwords have
    // to be hashed by Identity at runtime — a hash cannot be hard-coded into a migration.
    // So this runs once at startup instead, and does nothing if the data already exists.
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration,
            bool isDevelopment = false)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(IdentitySeeder));

            // 1. Make sure both roles exist.
            foreach (var role in new[] { UserRoles.Admin, UserRoles.Customer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Create the real administrator from configuration.
            //
            // The credentials come from configuration, NOT from constants in this file, so no
            // password is ever committed to git. Set them with User Secrets:
            //   dotnet user-secrets set "AdminUser:Email"    "you@example.com"
            //   dotnet user-secrets set "AdminUser:Password" "<a strong password>"
            var adminEmail = configuration["AdminUser:Email"];
            var adminPassword = configuration["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning(
                    "No admin account seeded: AdminUser:Email and AdminUser:Password are not configured. " +
                    "Set them with 'dotnet user-secrets set' to create the first administrator.");
            }
            else
            {
                await CreateUserAsync(userManager, logger,
                    adminEmail, adminPassword, "Administraator", UserRoles.Admin);
            }

            // 3. Demo accounts — DEVELOPMENT ONLY.
            //
            // These exist so the site can be tried out as both an administrator and an ordinary
            // customer without anyone having to register first, and so a fresh clone is usable
            // immediately.
            //
            // THE ENVIRONMENT CHECK IS THE WHOLE SAFETY MECHANISM. These passwords are written in
            // plain text below and are therefore public — anyone who reads the repository knows
            // them. Creating them on a real server would hand an attacker an administrator account.
            // Never remove this guard, and never reuse these passwords anywhere real.
            if (isDevelopment)
            {
                await CreateUserAsync(userManager, logger,
                    DemoAdminEmail, DemoAdminPassword, "Demo Administraator", UserRoles.Admin);

                await CreateUserAsync(userManager, logger,
                    DemoCustomerEmail, DemoCustomerPassword, "Demo Klient", UserRoles.Customer);

                // Printed at startup so the credentials are visible in the console rather than
                // having to be looked up in this file.
                logger.LogInformation(
                    "Development demo accounts available — admin: {AdminEmail} / {AdminPassword} · customer: {CustomerEmail} / {CustomerPassword}",
                    DemoAdminEmail, DemoAdminPassword, DemoCustomerEmail, DemoCustomerPassword);
            }
        }

        // ── DEMO CREDENTIALS (development only — see the guard above) ────────────────
        public const string DemoAdminEmail       = "admin@demo.local";
        public const string DemoAdminPassword    = "Admin123";

        public const string DemoCustomerEmail    = "klient@demo.local";
        public const string DemoCustomerPassword = "Klient123";

        // Creates one user and puts them in a role. Does nothing if the account already exists,
        // so restarting the application never duplicates anyone or resets a changed password.
        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            ILogger logger,
            string email,
            string password,
            string fullName,
            string role)
        {
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                // Most often the password fails the strength rules configured in Program.cs.
                logger.LogError("Failed to seed {Role} account {Email}: {Errors}",
                    role, email, string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);

            if (roleResult.Succeeded)
            {
                logger.LogInformation("Seeded {Role} account {Email}.", role, email);
            }
            else
            {
                // An account with no role is a confusing half-created state: the person can sign
                // in but is treated as though they never registered. Remove it and say so.
                await userManager.DeleteAsync(user);
                logger.LogError("Could not assign role {Role} to {Email}; the account was removed.",
                    role, email);
            }
        }
    }
}
