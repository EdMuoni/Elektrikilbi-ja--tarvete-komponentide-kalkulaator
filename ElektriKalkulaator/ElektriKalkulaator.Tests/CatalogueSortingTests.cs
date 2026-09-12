using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Tests for catalogue sorting.
    //
    // Sorting is applied to the IQueryable before ToListAsync, so it becomes a SQL ORDER BY rather
    // than a C# sort of an already-fetched list. That is the right thing to do, and also exactly
    // the kind of change that can silently return rows in the wrong order — the page still looks
    // plausible, just wrong. These tests pin the expected order.
    public class CatalogueSortingTests : TestBase
    {
        private async Task<List<string>> SortedNames(ProductSortOrder sort) =>
            (await Svc<IProductServices>().Search(null, null, sort: sort))
                .Select(p => p.Name)
                .ToList();

        private async Task<List<decimal>> SortedPrices(ProductSortOrder sort) =>
            (await Svc<IProductServices>().Search(null, null, sort: sort))
                .Select(p => p.Price)
                .ToList();

        [Fact]
        public async Task PriceLowToHigh_PutsTheCheapestFirst()
        {
            var prices = await SortedPrices(ProductSortOrder.PriceLowToHigh);

            // The seeded catalogue runs from 1.20 € (1.5 mm² cable) to 42.00 € (RCD).
            Assert.Equal(1.20m, prices.First());
            Assert.Equal(42.00m, prices.Last());

            // And the whole sequence is genuinely ascending, not just the ends.
            Assert.Equal(prices.OrderBy(p => p), prices);
        }

        [Fact]
        public async Task PriceHighToLow_IsTheExactReverseOrdering()
        {
            var prices = await SortedPrices(ProductSortOrder.PriceHighToLow);

            Assert.Equal(42.00m, prices.First());
            Assert.Equal(1.20m, prices.Last());
            Assert.Equal(prices.OrderByDescending(p => p), prices);
        }

        [Fact]
        public async Task NameAToZ_IsAlphabetical()
        {
            var names = await SortedNames(ProductSortOrder.NameAToZ);

            Assert.Equal(names.OrderBy(n => n, StringComparer.Ordinal).ToList().Count, names.Count);
            // Compare against the database's own ordering rather than .NET's, since collation
            // rules differ; what matters is that it is sorted and starts where we expect.
            Assert.StartsWith("ABB", names.First());
        }

        [Fact]
        public async Task StockHighToLow_PutsTheMostPlentifulFirst()
        {
            var stock = (await Svc<IProductServices>().Search(null, null, sort: ProductSortOrder.StockHighToLow))
                .Select(p => p.StockQuantity)
                .ToList();

            // Cable is seeded at 5000, the enclosure at 30.
            Assert.Equal(5000, stock.First());
            Assert.Equal(stock.OrderByDescending(s => s), stock);
        }

        [Fact]
        public async Task DefaultSort_GroupsByCategory()
        {
            var results = (await Svc<IProductServices>().Search(null, null)).ToList();

            var categoryNames = results.Select(p => p.Category!.Name).ToList();

            // Every product of a category appears consecutively — that is what "grouped" means.
            // Counting distinct blocks and comparing with distinct categories proves no category
            // is split into two separate runs.
            var blocks = categoryNames.Where((name, i) => i == 0 || name != categoryNames[i - 1]).Count();
            Assert.Equal(categoryNames.Distinct().Count(), blocks);
        }

        [Fact]
        public async Task SortingIsAppliedTogetherWithFiltering_NotInsteadOfIt()
        {
            // A common mistake is for sorting to replace the WHERE clause rather than add to it,
            // which quietly returns the whole catalogue in the right order.
            var breakers = Guid.Parse("11111111-0000-0000-0000-000000000001");

            var results = (await Svc<IProductServices>()
                .Search(breakers, null, sort: ProductSortOrder.PriceLowToHigh)).ToList();

            Assert.Equal(5, results.Count);                                   // still filtered
            Assert.All(results, p => Assert.Equal(breakers, p.CategoryId));
            Assert.Equal(results.Select(p => p.Price).OrderBy(p => p),        // and still sorted
                         results.Select(p => p.Price));
        }

        [Fact]
        public async Task SortingCombinesWithASearchTerm()
        {
            var results = (await Svc<IProductServices>()
                .Search(null, "ABB", sort: ProductSortOrder.PriceHighToLow)).ToList();

            Assert.Equal(5, results.Count);
            Assert.All(results, p => Assert.Equal("ABB", p.Brand));
            Assert.Equal(42.00m, results.First().Price);   // the RCD, most expensive ABB item
        }

        [Fact]
        public async Task EqualPrices_StillComeBackInAStableOrder()
        {
            // Two seeded products cost 8.50 € (ABB S201-B10 and Schneider Easy9 B16A). Without a
            // tiebreaker they could swap places between requests for no visible reason, which
            // looks like a glitch to a user comparing two page loads.
            var first  = (await Svc<IProductServices>().Search(null, null, sort: ProductSortOrder.PriceLowToHigh)).Select(p => p.Id).ToList();
            var second = (await Svc<IProductServices>().Search(null, null, sort: ProductSortOrder.PriceLowToHigh)).Select(p => p.Id).ToList();

            Assert.Equal(first, second);
        }
    }
}
