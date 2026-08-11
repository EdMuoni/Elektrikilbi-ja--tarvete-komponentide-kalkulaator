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
        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
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

            // 2. Create the first admin, if there isn't one yet.
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
                return;
            }

            if (await userManager.FindByEmailAsync(adminEmail) != null)
                return; // already created on a previous run

            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administraator",
                EmailConfirmed = true,
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                logger.LogInformation("Seeded administrator account {Email}.", adminEmail);
            }
            else
            {
                // Most often the configured password fails the strength rules in Program.cs.
                logger.LogError("Failed to seed administrator: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
