using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Tests for CategoryServices, in particular Delete, which used to throw a bare Exception
    // (producing a blank 500 page) and now reports what happened with a CategoryDeleteResult.
    //
    // Delete is not yet wired to any page, so these tests are the only thing exercising it. That
    // is deliberate: whoever builds the admin "delete category" button will inherit a method whose
    // behaviour is already pinned down.
    public class CategoryServicesTests : TestBase
    {
        [Fact]
        public async Task GetAll_ReturnsTheSeededCategories()
        {
            var categories = await Svc<ICategoryServices>().GetAll();
            Assert.Equal(5, categories.Count());
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenTheCategoryDoesNotExist()
        {
            var result = await Svc<ICategoryServices>().GetById(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_ForAnIdThatDoesNotExist()
        {
            var result = await Svc<ICategoryServices>().Delete(Guid.NewGuid());
            Assert.Equal(CategoryDeleteResult.NotFound, result);
        }

        [Fact]
        public async Task Delete_RefusesWhenTheCategoryStillHasProducts()
        {
            // "Kaitselülitid" holds five seeded breakers. Deleting it would leave those products
            // pointing at a category that no longer exists, so it must be refused.
            var breakers = Guid.Parse("11111111-0000-0000-0000-000000000001");

            var result = await Svc<ICategoryServices>().Delete(breakers);

            Assert.Equal(CategoryDeleteResult.StillHasProducts, result);

            // And crucially: nothing was deleted.
            Assert.NotNull(await Svc<ICategoryServices>().GetById(breakers));
        }

        [Fact]
        public async Task Delete_SucceedsForAnEmptyCategory()
        {
            // "Klemmid" is seeded but has no products attached, so it can be removed.
            var terminals = Guid.Parse("11111111-0000-0000-0000-000000000004");

            var result = await Svc<ICategoryServices>().Delete(terminals);

            Assert.Equal(CategoryDeleteResult.Deleted, result);
            Assert.Null(await Svc<ICategoryServices>().GetById(terminals));
        }

        [Fact]
        public async Task Create_AssignsAnIdWhenNoneWasSet()
        {
            var created = await Svc<ICategoryServices>().Create(new ProductCategory
            {
                Name = "Test kategooria",
                Description = "Loodud testis"
            });

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.NotNull(await Svc<ICategoryServices>().GetById(created.Id));
        }
    }
}
