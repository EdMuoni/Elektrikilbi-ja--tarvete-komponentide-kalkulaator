using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ElektriKalkulaator.Tests.Integration
{
    // Starts the REAL application in memory so tests can send it real HTTP requests.
    //
    // WHAT THIS IS FOR
    // The other test classes call services directly. That cannot check anything that only happens
    // once the whole web pipeline is running: whether an unauthenticated visitor is redirected to
    // the login page, whether a POST without an antiforgery token is refused, whether a URL returns
    // 404 or 302. Those behaviours come from middleware and attributes, not from our methods.
    //
    // WebApplicationFactory boots the actual Program.cs — the same DI registrations, the same
    // middleware order, the same [Authorize] attributes — and hands back an HttpClient wired
    // straight to it. No port is opened and no browser is involved, so it is fast and reliable.
    //
    // The ONLY thing swapped out is the database: SQL Server is replaced with an in-memory one, so
    // the tests need no server installed and cannot touch real data.
    public class TestWebAppFactory : WebApplicationFactory<Program>
    {
        // Every factory instance gets its own database, so test classes cannot see each other's rows.
        private readonly string _databaseName = $"IntegrationTest_{Guid.NewGuid()}";

        // Credentials for the admin account seeded into this test host.
        public const string AdminEmail    = "admin@test.local";
        public const string AdminPassword = "AdminTest123";

        public const string CustomerEmail    = "customer@test.local";
        public const string CustomerPassword = "CustomerTest123";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            // Supply the admin credentials the seeder looks for, the same way User Secrets do in
            // real use. Without these the seeder logs a warning and creates no admin.
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AdminUser:Email"]    = AdminEmail,
                    ["AdminUser:Password"] = AdminPassword
                });
            });

            builder.ConfigureServices(services =>
            {
                // Program.cs registered a SQL Server DbContext. Every trace of that registration
                // has to go before an in-memory one can be added, otherwise EF Core sees two
                // database providers at once and refuses to start with:
                //
                //   "Services for database providers 'SqlServer', 'InMemory' have been registered
                //    in the service provider. Only a single database provider can be registered."
                //
                // Three kinds of registration need removing, and missing any one of them causes
                // that error:
                //   - DbContextOptions<T> and DbContextOptions — the resolved options objects
                //   - the DbContext itself
                //   - IDbContextOptionsConfiguration<T> — added by AddDbContext in .NET 9 to hold
                //     the "use SQL Server" callback. This one is easy to miss because it did not
                //     exist in earlier versions, and it is what actually carries the provider.
                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<ElektriKalkulaatorContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType == typeof(ElektriKalkulaatorContext) ||
                        (d.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") ?? false) ||
                        (d.ServiceType.FullName?.Contains("EntityFrameworkCore.SqlServer") ?? false))
                    .ToList();

                foreach (var descriptor in toRemove)
                    services.Remove(descriptor);

                services.AddDbContext<ElektriKalkulaatorContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }

        // Creates an HttpClient that does NOT follow redirects.
        //
        // This matters: a test checking "an anonymous visitor is sent to the login page" needs to
        // see the 302 and its Location header. A client that follows redirects automatically would
        // report 200 for the login page instead, and the test would pass for the wrong reason.
        public HttpClient CreateNonRedirectingClient() =>
            CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Adds a customer account to this host. The admin is created by the application's own
        // seeder; a customer has to be made here because normally one registers through the form.
        public async Task EnsureCustomerExistsAsync()
        {
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByEmailAsync(CustomerEmail) != null)
                return;

            var user = new ApplicationUser
            {
                UserName = CustomerEmail,
                Email = CustomerEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, CustomerPassword);
            Assert.True(result.Succeeded,
                "Could not create the test customer: " + string.Join("; ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, UserRoles.Customer);
        }
    }
}
