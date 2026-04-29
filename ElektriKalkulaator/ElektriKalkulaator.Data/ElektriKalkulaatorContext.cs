using ElektriKalkulaator.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace ElektriKalkulaator.Data
{
    public class ElektriKalkulaatorContext : DbContext
    {
        public ElektriKalkulaatorContext(DbContextOptions<ElektriKalkulaatorContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<CalculationRule> CalculationRules { get; set; }
        public DbSet<PowerboxCalculation> PowerboxCalculations { get; set; }
        public DbSet<PowerboxRequirements> PowerboxRequirements { get; set; }
        public DbSet<PowerboxComponents> PowerboxComponents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<PowerboxRequirements>()
                .HasOne(r => r.Calculation)
                .WithOne(c => c.Requirements)
                .HasForeignKey<PowerboxRequirements>(r => r.CalculationId);

            modelBuilder.Entity<PowerboxComponents>()
                .HasOne(c => c.Calculation)
                .WithMany(calc => calc.Components)
                .HasForeignKey(c => c.CalculationId);

            modelBuilder.Entity<PowerboxComponents>()
                .HasOne(c => c.Product)
                .WithMany(p => p.PowerboxComponents)
                .HasForeignKey(c => c.ProductId);

            // Decimal precision (avoids SQL Server default of 18,2 for everything)
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(10, 2);
            modelBuilder.Entity<Product>().Property(p => p.RatedCurrent).HasPrecision(10, 2);
            modelBuilder.Entity<Product>().Property(p => p.WireCrossSectionMm2).HasPrecision(5, 2);
            modelBuilder.Entity<CalculationRule>().Property(r => r.WireCrossSectionMm2).HasPrecision(5, 2);
            modelBuilder.Entity<PowerboxComponents>().Property(c => c.UnitPrice).HasPrecision(10, 2);
            modelBuilder.Entity<PowerboxComponents>().Property(c => c.TotalPrice).HasPrecision(10, 2);
            modelBuilder.Entity<PowerboxComponents>().Property(c => c.WireCrossSectionMm2).HasPrecision(5, 2);
            modelBuilder.Entity<PowerboxCalculation>().Property(c => c.TotalCost).HasPrecision(10, 2);

            // Seed data — fixed GUIDs so migrations don't re-insert on every run
            SeedCategories(modelBuilder);
            SeedProducts(modelBuilder);
            SeedCalculationRules(modelBuilder);
        }

        private void SeedCategories(ModelBuilder mb)
        {
            // Category names are in Estonian — the calculator service matches on these exact strings
            mb.Entity<ProductCategory>().HasData(
                new ProductCategory { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), Name = "Kaitselülitid",          Description = "Automaatkaitselülitid (MCB) ABB, Schneider, Hager", CreatedAt = new DateTime(2026, 1, 1), ModifiedAt = new DateTime(2026, 1, 1) },
                new ProductCategory { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), Name = "Juhtmed",                Description = "NYM-J ja NYY kaablid 1.5mm², 2.5mm², 6mm²",          CreatedAt = new DateTime(2026, 1, 1), ModifiedAt = new DateTime(2026, 1, 1) },
                new ProductCategory { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), Name = "RCD / Rikkevoolukaitsmeid", Description = "Rikkevoolukaitsmed (RCD/RCCB) 25A, 40A, 63A",    CreatedAt = new DateTime(2026, 1, 1), ModifiedAt = new DateTime(2026, 1, 1) },
                new ProductCategory { Id = Guid.Parse("11111111-0000-0000-0000-000000000004"), Name = "Klemmid",                Description = "Ühenduskarbid ja klemmiribad",                        CreatedAt = new DateTime(2026, 1, 1), ModifiedAt = new DateTime(2026, 1, 1) },
                new ProductCategory { Id = Guid.Parse("11111111-0000-0000-0000-000000000005"), Name = "Kilbi korpused",         Description = "Plastik ja metallist kilbi korpused 4-24 mooduli jaoks", CreatedAt = new DateTime(2026, 1, 1), ModifiedAt = new DateTime(2026, 1, 1) }
            );
        }

        private void SeedProducts(ModelBuilder mb)
        {
            var cat1 = Guid.Parse("11111111-0000-0000-0000-000000000001"); // Kaitselülitid
            var cat2 = Guid.Parse("11111111-0000-0000-0000-000000000002"); // Juhtmed
            var cat3 = Guid.Parse("11111111-0000-0000-0000-000000000003"); // RCD
            var cat5 = Guid.Parse("11111111-0000-0000-0000-000000000005"); // Kilbi korpused
            var now  = new DateTime(2026, 1, 1);

            mb.Entity<Product>().HasData(
                // Breakers — 10A (lighting)
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000001"), CategoryId = cat1, Name = "ABB S201-B10 Kaitselüliti 10A",  Brand = "ABB",       RatedCurrent = 10, Voltage = 230, Price = 8.50m,  StockQuantity = 150,  WireCrossSectionMm2 = 1.5m, Description = "1-pooluseline kaitselüliti B10A, 6kA", CreatedAt = now, ModifiedAt = now },
                // Breakers — 16A (sockets)
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000002"), CategoryId = cat1, Name = "ABB S201-B16 Kaitselüliti 16A",  Brand = "ABB",       RatedCurrent = 16, Voltage = 230, Price = 9.20m,  StockQuantity = 200,  WireCrossSectionMm2 = 2.5m, Description = "1-pooluseline kaitselüliti B16A, 6kA", CreatedAt = now, ModifiedAt = now },
                // Breakers — 32A (stove)
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000003"), CategoryId = cat1, Name = "ABB S201-B32 Kaitselüliti 32A",  Brand = "ABB",       RatedCurrent = 32, Voltage = 230, Price = 12.80m, StockQuantity = 80,   WireCrossSectionMm2 = 6.0m, Description = "1-pooluseline kaitselüliti B32A, 6kA", CreatedAt = now, ModifiedAt = now },
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000004"), CategoryId = cat1, Name = "Schneider Easy9 B10A",           Brand = "Schneider", RatedCurrent = 10, Voltage = 230, Price = 7.90m,  StockQuantity = 120,  WireCrossSectionMm2 = 1.5m, Description = "1-pooluseline kaitselüliti B10A",      CreatedAt = now, ModifiedAt = now },
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000005"), CategoryId = cat1, Name = "Schneider Easy9 B16A",           Brand = "Schneider", RatedCurrent = 16, Voltage = 230, Price = 8.50m,  StockQuantity = 180,  WireCrossSectionMm2 = 2.5m, Description = "1-pooluseline kaitselüliti B16A",      CreatedAt = now, ModifiedAt = now },
                // Cables — sold by the metre
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000006"), CategoryId = cat2, Name = "NYM-J 3x1.5mm² kaabel (1m)",    Brand = "Draka",     RatedCurrent = 10, Voltage = 230, Price = 1.20m,  StockQuantity = 5000, WireCrossSectionMm2 = 1.5m, Description = "Valgustuse juhe, müüakse meetrites",   CreatedAt = now, ModifiedAt = now },
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000007"), CategoryId = cat2, Name = "NYM-J 3x2.5mm² kaabel (1m)",    Brand = "Draka",     RatedCurrent = 16, Voltage = 230, Price = 1.85m,  StockQuantity = 5000, WireCrossSectionMm2 = 2.5m, Description = "Pistikute juhe, müüakse meetrites",    CreatedAt = now, ModifiedAt = now },
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000008"), CategoryId = cat2, Name = "NYM-J 3x6mm² kaabel (1m)",      Brand = "Draka",     RatedCurrent = 32, Voltage = 230, Price = 3.60m,  StockQuantity = 2000, WireCrossSectionMm2 = 6.0m, Description = "Pliidi juhe, müüakse meetrites",       CreatedAt = now, ModifiedAt = now },
                // RCD
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000009"), CategoryId = cat3, Name = "ABB F202 AC-40/0.03 RCD 40A",   Brand = "ABB",       RatedCurrent = 40, Voltage = 230, Price = 42.00m, StockQuantity = 50,   Description = "2-pooluseline rikkevoolukaitsme 40A 30mA",  CreatedAt = now, ModifiedAt = now },
                // Enclosure
                new Product { Id = Guid.Parse("22222222-0000-0000-0000-000000000010"), CategoryId = cat5, Name = "ABB Mistral41F 12 mooduli kilp", Brand = "ABB",       RatedCurrent = 0,  Voltage = 230, Price = 28.50m, StockQuantity = 30,   Description = "Pinna-paigaldusega kilbi korpus 12 moodulile", CreatedAt = now, ModifiedAt = now }
            );
        }

        private void SeedCalculationRules(ModelBuilder mb)
        {
            var now = new DateTime(2026, 1, 1);

            // BuildingType values must match exactly what the form sends ("korterelamu" / "eramu" / "ärihoone")
            mb.Entity<CalculationRule>().HasData(
                new CalculationRule { Id = Guid.Parse("33333333-0000-0000-0000-000000000001"), RuleName = "Korterelamu valgustus", BuildingType = "korterelamu", CircuitType = "lighting", WireCrossSectionMm2 = 1.5m, BreakerAmperes = 10, RoomsFrom = 1, RoomsTo = 999, CreatedAt = now, ModifiedAt = now },
                new CalculationRule { Id = Guid.Parse("33333333-0000-0000-0000-000000000002"), RuleName = "Korterelamu pistikud",  BuildingType = "korterelamu", CircuitType = "socket",   WireCrossSectionMm2 = 2.5m, BreakerAmperes = 16, RoomsFrom = 1, RoomsTo = 999, CreatedAt = now, ModifiedAt = now },
                new CalculationRule { Id = Guid.Parse("33333333-0000-0000-0000-000000000003"), RuleName = "Korterelamu pliit",     BuildingType = "korterelamu", CircuitType = "stove",    WireCrossSectionMm2 = 6.0m, BreakerAmperes = 32, RoomsFrom = 1, RoomsTo = 999, CreatedAt = now, ModifiedAt = now },
                new CalculationRule { Id = Guid.Parse("33333333-0000-0000-0000-000000000004"), RuleName = "Eramu valgustus",       BuildingType = "eramu",       CircuitType = "lighting", WireCrossSectionMm2 = 1.5m, BreakerAmperes = 10, RoomsFrom = 1, RoomsTo = 999, CreatedAt = now, ModifiedAt = now },
                new CalculationRule { Id = Guid.Parse("33333333-0000-0000-0000-000000000005"), RuleName = "Eramu pistikud",        BuildingType = "eramu",       CircuitType = "socket",   WireCrossSectionMm2 = 2.5m, BreakerAmperes = 16, RoomsFrom = 1, RoomsTo = 999, CreatedAt = now, ModifiedAt = now },
                new CalculationRule { Id = Guid.Parse("33333333-0000-0000-0000-000000000006"), RuleName = "Eramu pliit",           BuildingType = "eramu",       CircuitType = "stove",    WireCrossSectionMm2 = 6.0m, BreakerAmperes = 32, RoomsFrom = 1, RoomsTo = 999, CreatedAt = now, ModifiedAt = now },
                new CalculationRule { Id = Guid.Parse("33333333-0000-0000-0000-000000000007"), RuleName = "Ärihoone valgustus",    BuildingType = "ärihoone",    CircuitType = "lighting", WireCrossSectionMm2 = 1.5m, BreakerAmperes = 10, RoomsFrom = 1, RoomsTo = 999, CreatedAt = now, ModifiedAt = now },
                new CalculationRule { Id = Guid.Parse("33333333-0000-0000-0000-000000000008"), RuleName = "Ärihoone pistikud",     BuildingType = "ärihoone",    CircuitType = "socket",   WireCrossSectionMm2 = 2.5m, BreakerAmperes = 16, RoomsFrom = 1, RoomsTo = 999, CreatedAt = now, ModifiedAt = now }
            );
        }
    }
}
