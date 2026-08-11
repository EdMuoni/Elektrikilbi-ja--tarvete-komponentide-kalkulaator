using ElektriKalkulaator.ApplicationServices.Services;
using ElektriKalkulaator.Core.ServiceInterface;
using ElektriKalkulaator.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ElektriKalkulaator.Tests
{
    // Every test class below inherits from this one. It sets up a tiny, throwaway copy of the
    // app's dependency injection container — the same services (ICalculatorServices,
    // IProductServices, ...) that Program.cs wires up for the real website — but pointed at an
    // in-memory database instead of the real SQL Server one.
    //
    // Why an in-memory database instead of the real one?
    //   - Tests run in milliseconds and don't need SQL Server installed on whoever's machine
    //     is running them (including CI servers).
    //   - Every test starts from a known, predictable state.
    //   - We are still testing the REAL CalculatorServices/ProductServices code — only the
    //     database underneath is swapped out. If the calculator's math is wrong, these tests
    //     will still catch it.
    //
    // Each test method gets its own brand-new database (a random name, see UseInMemoryDatabase
    // below), so tests can never see each other's leftover data and can run in any order.
    public abstract class TestBase
    {
        // The "seed data" already baked into ElektriKalkulaatorContext (the categories, products,
        // and EVS-HD 60364 calculation rules — see ElektriKalkulaatorContext.OnModelCreating) is
        // available here too, since it's part of the same DbContext model. Tests can rely on it
        // directly instead of re-typing the same product/rule data by hand.
        protected ElektriKalkulaatorContext Context { get; }

        private readonly IServiceProvider _serviceProvider;

        protected TestBase()
        {
            var services = new ServiceCollection();

            // Register the same services the real app registers in Program.cs. If this list
            // drifts out of sync with Program.cs, that's a sign these tests need updating.
            services.AddScoped<ICalculatorServices, CalculatorServices>();
            services.AddScoped<IProductServices, ProductServices>();
            services.AddScoped<ICategoryServices, CategoryServices>();

            services.AddDbContext<ElektriKalkulaatorContext>(options =>
            {
                // A unique database name per test class instance = full isolation between tests.
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());

                // The in-memory provider doesn't support real transactions and logs a warning
                // every time SaveChanges() is called because of it. That's expected and harmless
                // here, so we tell EF Core not to warn about it — otherwise it clutters test output.
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            _serviceProvider = services.BuildServiceProvider();

            Context = _serviceProvider.GetRequiredService<ElektriKalkulaatorContext>();

            // EnsureCreated() builds the in-memory database from the model — including running
            // the HasData(...) seed calls in OnModelCreating. Without this, the database would
            // exist but be completely empty.
            Context.Database.EnsureCreated();
        }

        // Shortcut so tests can write Svc<ICalculatorServices>() instead of spelling out the
        // full service-provider lookup every time.
        protected T Svc<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
    }
}
