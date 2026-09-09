using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // The brand filter, added after looking at how real electrical wholesalers are actually
    // browsed. Trade catalogues are navigated by BRAND at least as often as by category: an
    // electrician often knows they want Schneider before they know which category the part is
    // filed under, because the brand is what is already installed in the building.
    //
    // The subtlety worth testing is that `brand` and `searchTerm` are deliberately different
    // kinds of match:
    //
    //   searchTerm  partial  - "ABB" finds "ABB S201-B32", and also anything whose NAME
    //                          contains "ABB". It is a search box.
    //   brand       exact    - "ABB" returns only products whose Brand is exactly "ABB".
    //                          It is a filter chosen from a known list.
    //
    // Getting that wrong is silent: a partial brand filter still returns plausible-looking
    // results, just with other brands mixed in, and nobody notices until a customer orders the
    // wrong part.
    public class CatalogueBrandFilterTests : TestBase
    {
        [Fact]
        public async Task GetBrands_ReturnsEachBrandOnceInAlphabeticalOrder()
        {
            var brands = (await Svc<IProductServices>().GetBrands()).ToList();

            Assert.NotEmpty(brands);
            Assert.Equal(brands.Distinct().Count(), brands.Count);      // no duplicates
            Assert.Equal(brands.OrderBy(b => b).ToList(), brands);      // already sorted
        }

        [Fact]
        public async Task GetBrands_OffersOnlyBrandsThatHaveProducts()
        {
            // The list drives a filter in the UI. If it can offer a brand with no products, the
            // visitor can click a filter that returns an empty page, which reads as a broken site.
            var svc = Svc<IProductServices>();

            foreach (var brand in await svc.GetBrands())
            {
                var hits = await svc.Search(null, null, brand);
                Assert.True(hits.Any(), $"Brand '{brand}' is offered but matches no products.");
            }
        }

        [Fact]
        public async Task FilteringByBrand_ReturnsOnlyThatBrand()
        {
            var svc = Svc<IProductServices>();
            var brand = (await svc.GetBrands()).First();

            var results = (await svc.Search(null, null, brand)).ToList();

            Assert.NotEmpty(results);
            Assert.All(results, p => Assert.Equal(brand, p.Brand));
        }

        [Fact]
        public async Task TheBrandFilter_IsExactAndNotAPartialMatch()
        {
            // THE IMPORTANT ONE. A prefix of a real brand must match nothing, because the filter
            // is an exact comparison. If this ever returns rows, the filter has silently become
            // a "contains" search and one brand's filter can include another's products.
            var svc = Svc<IProductServices>();
            var brand = (await svc.GetBrands()).First();
            var prefix = brand[..^1];                    // "ABB" -> "AB"

            var results = await svc.Search(null, null, prefix);

            Assert.Empty(results);
        }

        [Fact]
        public async Task BrandCombinesWithCategoryAndSearchRatherThanReplacingThem()
        {
            // Filters must narrow together. An earlier version of the catalogue had controls that
            // each reset the others, so choosing a sort order threw away the category filter.
            var svc = Svc<IProductServices>();
            var brand = (await svc.GetBrands()).First();

            var byBrand = (await svc.Search(null, null, brand)).ToList();
            var category = byBrand.First().CategoryId;

            var both = (await svc.Search(category, null, brand)).ToList();

            Assert.NotEmpty(both);
            Assert.All(both, p =>
            {
                Assert.Equal(brand, p.Brand);
                Assert.Equal(category, p.CategoryId);
            });
            Assert.True(both.Count <= byBrand.Count, "Adding a filter must never widen the results.");
        }

        [Fact]
        public async Task AnEmptyOrWhitespaceBrand_MeansAllBrands()
        {
            // The query string carries brand="" when the visitor clears the filter, and that must
            // behave like "no filter" rather than "a brand whose name is empty".
            var svc = Svc<IProductServices>();
            var all = (await svc.Search(null, null)).Count();

            Assert.Equal(all, (await svc.Search(null, null, "")).Count());
            Assert.Equal(all, (await svc.Search(null, null, "   ")).Count());
        }

        [Fact]
        public async Task BrandIsTrimmed_SoALinkWithStrayWhitespaceStillWorks()
        {
            var svc = Svc<IProductServices>();
            var brand = (await svc.GetBrands()).First();

            var padded = (await svc.Search(null, null, "  " + brand + "  ")).ToList();

            Assert.NotEmpty(padded);
            Assert.All(padded, p => Assert.Equal(brand, p.Brand));
        }
    }
}
