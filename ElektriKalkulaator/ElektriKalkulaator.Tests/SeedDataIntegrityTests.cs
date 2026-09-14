using ElektriKalkulaator.Core.Domain;

namespace ElektriKalkulaator.Tests
{
    // Checks that the seeded demo data is internally consistent.
    //
    // Seed data is easy to get subtly wrong: a typo in a category name silently breaks the
    // calculator (it matches categories by exact string), a price of 0 makes the BOM total
    // meaningless, and an ImagePath pointing at a file that was never committed shows a broken
    // image to everyone who clones the repository — which is exactly the bug found on 2026-08-11.
    //
    // These tests are cheap and catch a whole class of mistakes that a compiler never will.
    public class SeedDataIntegrityTests : TestBase
    {
        // Finds wwwroot by walking up from the test binary to the solution folder. The tests run
        // from bin/Debug/net9.0, so the web project sits a few levels up.
        private static string WebRootPath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "ElektriKalkulaator", "wwwroot")))
                dir = dir.Parent;

            Assert.NotNull(dir); // if this fails the test project has been moved
            return Path.Combine(dir!.FullName, "ElektriKalkulaator", "wwwroot");
        }

        [Fact]
        public void EverySeededProduct_HasAnImageFileThatActuallyExistsOnDisk()
        {
            // THE IMPORTANT ONE. A .gitignore rule once excluded the product photos while the seed
            // data still referenced them, so a fresh clone rendered ten broken images. Nothing in
            // the build or the other tests noticed. This test would have.
            var webRoot = WebRootPath();

            var missing = Context.Products
                .Where(p => p.ImagePath != null)
                .AsEnumerable()
                .Select(p => new
                {
                    p.Name,
                    p.ImagePath,
                    FullPath = Path.Combine(webRoot, p.ImagePath!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
                })
                .Where(x => !File.Exists(x.FullPath))
                .Select(x => $"{x.Name} -> {x.ImagePath}")
                .ToList();

            Assert.True(missing.Count == 0,
                "These seeded products reference image files that do not exist:\n" + string.Join("\n", missing));
        }

        [Fact]
        public void EverySeededProduct_BelongsToACategoryThatExists()
        {
            var categoryIds = Context.ProductCategories.Select(c => c.Id).ToHashSet();

            var orphans = Context.Products
                .AsEnumerable()
                .Where(p => !categoryIds.Contains(p.CategoryId))
                .Select(p => p.Name)
                .ToList();

            Assert.True(orphans.Count == 0,
                "These products point at a category that does not exist: " + string.Join(", ", orphans));
        }

        [Fact]
        public void EverySeededProduct_HasAPositivePrice()
        {
            // A zero or negative price would silently corrupt every BOM total that includes it.
            Assert.All(Context.Products, p => Assert.True(p.Price > 0, $"{p.Name} has price {p.Price}"));
        }

        [Fact]
        public void EverySeededProduct_HasANameAndABrand()
        {
            Assert.All(Context.Products, p =>
            {
                Assert.False(string.IsNullOrWhiteSpace(p.Name));
                Assert.False(string.IsNullOrWhiteSpace(p.Brand));
            });
        }

        [Fact]
        public void EverySeededProduct_HasNonNegativeStock()
        {
            Assert.All(Context.Products, p => Assert.True(p.StockQuantity >= 0, $"{p.Name} has stock {p.StockQuantity}"));
        }

        // The calculator finds products by matching Category.Name against these exact strings.
        // A rename or typo in the seed data would break the calculator without breaking the build.
        [Theory]
        [InlineData("Kaitselülitid")]
        [InlineData("Juhtmed")]
        [InlineData("RCD / Rikkevoolukaitsmeid")]
        [InlineData("Kilbi korpused")]
        public void CategoryNamesTheCalculatorDependsOn_ExistExactly(string requiredName)
        {
            Assert.True(Context.ProductCategories.Any(c => c.Name == requiredName),
                $"CalculatorServices looks for the category '{requiredName}' by exact name, but it is not in the seed data.");
        }

        [Fact]
        public void EachCategoryTheCalculatorNeeds_HasAtLeastOneInStockProduct()
        {
            // If any of these were empty or out of stock, the calculator would quietly leave that
            // component out of the BOM instead of failing — an easy problem to miss.
            foreach (var categoryName in new[] { "Kaitselülitid", "Juhtmed", "RCD / Rikkevoolukaitsmeid", "Kilbi korpused" })
            {
                var category = Context.ProductCategories.Single(c => c.Name == categoryName);
                var inStock = Context.Products.Count(p => p.CategoryId == category.Id && p.StockQuantity > 0);

                Assert.True(inStock > 0, $"Category '{categoryName}' has no in-stock products, so the calculator would silently skip it.");
            }
        }

        // Every building type offered in the UI must have rules, or the calculator returns an
        // empty BOM and the user sees nothing with no explanation.
        [Theory]
        [InlineData("korterelamu")]
        [InlineData("eramu")]
        [InlineData("ärihoone")]
        public void EveryBuildingTypeOfferedInTheForm_HasCalculationRules(string buildingType)
        {
            Assert.True(Context.CalculationRules.Any(r => r.BuildingType == buildingType),
                $"No calculation rules exist for building type '{buildingType}', so the calculator would return an empty result.");
        }

        [Fact]
        public void EveryCalculationRule_UsesAKnownCircuitType()
        {
            // Calculate() switches on this string; an unknown value falls through to 0 circuits
            // and the rule is silently ignored.
            var known = new[] { "lighting", "socket", "stove" };

            Assert.All(Context.CalculationRules, r =>
                Assert.True(known.Contains(r.CircuitType),
                    $"Rule '{r.RuleName}' has circuit type '{r.CircuitType}', which Calculate() does not recognise."));
        }

        [Fact]
        public void EveryCalculationRule_HasASensibleBreakerRatingAndCableSize()
        {
            Assert.All(Context.CalculationRules, r =>
            {
                Assert.True(r.BreakerAmperes > 0, $"Rule '{r.RuleName}' has breaker rating {r.BreakerAmperes}");
                Assert.True(r.WireCrossSectionMm2 > 0, $"Rule '{r.RuleName}' has cable size {r.WireCrossSectionMm2}");
            });
        }

        [Fact]
        public void EveryCalculationRule_HasAMatchingBreakerInTheCatalogue()
        {
            // A rule asking for a 25 A breaker when the catalogue only stocks 10/16/32 A would
            // produce a BOM missing its breaker line, with no error shown.
            var breakers = Context.ProductCategories.Single(c => c.Name == "Kaitselülitid");

            foreach (var rule in Context.CalculationRules.ToList())
            {
                var match = Context.Products.Any(p =>
                    p.CategoryId == breakers.Id &&
                    p.RatedCurrent == rule.BreakerAmperes &&
                    p.StockQuantity > 0);

                Assert.True(match, $"Rule '{rule.RuleName}' needs a {rule.BreakerAmperes} A breaker, but none is in stock.");
            }
        }

        [Fact]
        public void EveryCalculationRule_HasAMatchingCableInTheCatalogue()
        {
            var cables = Context.ProductCategories.Single(c => c.Name == "Juhtmed");

            foreach (var rule in Context.CalculationRules.ToList())
            {
                var match = Context.Products.Any(p =>
                    p.CategoryId == cables.Id &&
                    p.WireCrossSectionMm2 == rule.WireCrossSectionMm2 &&
                    p.StockQuantity > 0);

                Assert.True(match, $"Rule '{rule.RuleName}' needs {rule.WireCrossSectionMm2} mm² cable, but none is in stock.");
            }
        }

        [Fact]
        public void SeededIdsAreUnique()
        {
            // Fixed GUIDs are hand-written in the seed data, so a copy-paste slip is plausible.
            Assert.Equal(Context.Products.Count(), Context.Products.Select(p => p.Id).Distinct().Count());
            Assert.Equal(Context.ProductCategories.Count(), Context.ProductCategories.Select(c => c.Id).Distinct().Count());
            Assert.Equal(Context.CalculationRules.Count(), Context.CalculationRules.Select(r => r.Id).Distinct().Count());
        }
    }
}
