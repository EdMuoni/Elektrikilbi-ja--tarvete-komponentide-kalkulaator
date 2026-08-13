using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Tests for ProductServices.Search — the catalogue's category filter and text search.
    //
    // These matter because the filtering was moved out of C# and into the database query. That is
    // a performance improvement, but rewriting a filter is exactly the kind of change that can
    // quietly return the wrong rows. These tests pin the expected results.
    //
    // They run against the seeded demo catalogue (see TestBase), which contains 10 products:
    // 5 breakers (3 ABB + 2 Schneider), 3 Draka cables, 1 ABB RCD, 1 ABB enclosure.
    public class CatalogueSearchTests : TestBase
    {
        private static readonly Guid BreakersCategory = Guid.Parse("11111111-0000-0000-0000-000000000001");
        private static readonly Guid CablesCategory   = Guid.Parse("11111111-0000-0000-0000-000000000002");

        [Fact]
        public async Task NoFilters_ReturnsTheWholeCatalogue()
        {
            var results = await Svc<IProductServices>().Search(null, null);
            Assert.Equal(10, results.Count());
        }

        [Fact]
        public async Task CategoryFilter_ReturnsOnlyThatCategory()
        {
            var results = await Svc<IProductServices>().Search(CablesCategory, null);

            Assert.Equal(3, results.Count());
            Assert.All(results, p => Assert.Equal(CablesCategory, p.CategoryId));
        }

        [Fact]
        public async Task SearchTerm_MatchesBrand()
        {
            // "ABB" is a brand, not part of most product names — 3 breakers + RCD + enclosure.
            var results = await Svc<IProductServices>().Search(null, "ABB");
            Assert.Equal(5, results.Count());
        }

        [Fact]
        public async Task SearchTerm_MatchesProductName()
        {
            // "kaabel" appears in the three cable product names, not in any brand.
            var results = await Svc<IProductServices>().Search(null, "kaabel");
            Assert.Equal(3, results.Count());
        }

        [Fact]
        public async Task CategoryAndSearchTerm_AreCombinedWithAnd()
        {
            // Breakers made by ABB: 3 of the 5 breakers (the other 2 are Schneider).
            // If the filters were combined with OR instead of AND this would return far more.
            var results = await Svc<IProductServices>().Search(BreakersCategory, "ABB");

            Assert.Equal(3, results.Count());
            Assert.All(results, p => Assert.Equal(BreakersCategory, p.CategoryId));
            Assert.All(results, p => Assert.Equal("ABB", p.Brand));
        }

        [Fact]
        public async Task SearchTermThatMatchesNothing_ReturnsEmpty()
        {
            var results = await Svc<IProductServices>().Search(null, "zzzzzzzz");
            Assert.Empty(results);
        }

        [Fact]
        public async Task WhitespaceSearchTerm_IsIgnoredRatherThanMatchingNothing()
        {
            // A user pressing space in the search box should not empty the catalogue.
            var results = await Svc<IProductServices>().Search(null, "   ");
            Assert.Equal(10, results.Count());
        }

        [Fact]
        public async Task SearchTerm_IsTrimmed()
        {
            // Stray spaces around a pasted term must not stop it matching.
            var results = await Svc<IProductServices>().Search(null, "  Schneider  ");
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task Results_IncludeTheCategory_SoTheViewCanShowItsName()
        {
            // Products/Index.cshtml prints product.Category.Name. If the query stopped eager
            // loading Category that would silently render nothing, so assert it is populated.
            var results = await Svc<IProductServices>().Search(BreakersCategory, null);

            Assert.All(results, p => Assert.NotNull(p.Category));
        }
    }
}
