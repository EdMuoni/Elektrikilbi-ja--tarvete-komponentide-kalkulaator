using ElektriKalkulaator.ApplicationServices.Services;
using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.ServiceInterface;
using ElektriKalkulaator.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC with Razor views
builder.Services.AddControllersWithViews();

// SQL Server via connection string in appsettings.json
builder.Services.AddDbContext<ElektriKalkulaatorContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// ── AUTHENTICATION AND ROLES ────────────────────────────────────────────────
// AddIdentity registers everything needed to create users, check passwords and issue login
// cookies. AddRoles adds role support so pages can be limited with [Authorize(Roles = "Admin")].
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password rules. Length matters far more than forced symbol variety, so we require a
        // longer password instead of demanding punctuation nobody remembers.
        options.Password.RequiredLength         = 8;
        options.Password.RequireDigit           = true;
        options.Password.RequireLowercase       = true;
        options.Password.RequireUppercase       = true;
        options.Password.RequireNonAlphanumeric = false;

        // Lock an account for 5 minutes after 5 failed attempts. This is what makes guessing
        // passwords impractical.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(5);

        // No two accounts may share an email address.
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ElektriKalkulaatorContext>()
    .AddDefaultTokenProviders();

// Where to send people who are not signed in, or who lack the required role.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly  = true;   // JavaScript cannot read the login cookie
});

// Session for the shopping cart (server-side memory store, 30-min idle timeout)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

// Dependency injection — interface → implementation
builder.Services.AddScoped<ICalculatorServices, CalculatorServices>();
builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddScoped<ICategoryServices, CategoryServices>();

var app = builder.Build();

// Apply any pending EF Core migrations on startup (avoids manual Update-Database during dev)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ElektriKalkulaatorContext>();
    try
    {
        // Migrations only exist for a real database engine such as SQL Server. Automated tests
        // swap in an in-memory database, which has no migration support at all — asking it to
        // migrate throws. IsRelational() tells the two apart, so the same startup code works in
        // both places instead of the test host needing a special copy of it.
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            // In-memory: build the schema straight from the model, which also applies the seed
            // data declared in OnModelCreating.
            await context.Database.EnsureCreatedAsync();
        }

        // Create the Admin and Customer roles and, if configured, the first admin account.
        // Runs after migrations so the Identity tables definitely exist.
        //
        // The environment flag also enables the demo admin/customer accounts, which must NEVER be
        // created on a real server — their passwords are in the source code. See IdentitySeeder.
        await IdentitySeeder.SeedAsync(
            scope.ServiceProvider,
            builder.Configuration,
            app.Environment.IsDevelopment());
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration or seeding failed on startup.");

        // Stop instead of continuing. If the schema could not be brought up to date, every page
        // that touches the database will fail anyway — but with confusing errors scattered across
        // the site rather than one clear message here. Failing immediately, at the point where the
        // real cause was logged, is far easier to diagnose.
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Order matters and is not interchangeable:
//   UseAuthentication - works out WHO the visitor is, by reading the login cookie
//   UseAuthorization  - works out WHETHER they may open this page
// Authorization cannot decide anything if authentication has not run first. Previously only
// UseAuthorization was here, so every [Authorize] check would have had nobody to check against —
// which is why the admin pages were reachable by anyone.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Program.cs uses "top-level statements", so the compiler generates the Program class for us and
// makes it internal. WebApplicationFactory<Program> in the test project needs to refer to that
// class by name, so we declare it here as public. It stays empty on purpose — this exists only to
// change its visibility, not to add behaviour.
public partial class Program { }
