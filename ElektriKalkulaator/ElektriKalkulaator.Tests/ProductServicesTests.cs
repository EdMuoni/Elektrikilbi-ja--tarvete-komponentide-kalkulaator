using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Tests for ProductServices — including regression tests for a bug fixed in this same
    // session: GetById/Update/Delete used to be typed as always returning a non-null Product,
    // even though they could actually return null at runtime (the code just hid it with a `!`).
    // These tests exist so that bug can never come back unnoticed.
    public class ProductServicesTests : TestBase
    {
        // A category must exist before a product can reference it — grab one straight from the
        // seed data instead of creating a throwaway one in every test.
        private Guid ExistingCategoryId =>
            Context.ProductCategories.First().Id;

        private ProductDto NewProductDto() => new()
        {
            CategoryId = ExistingCategoryId,
            Name = "Test Toode",
            Brand = "TestBrand",
            RatedCurrent = 16,
            Voltage = 230,
            Price = 9.99m,
            StockQuantity = 5
        };

        [Fact]
        public async Task GetById_ReturnsNull_ForAnIdThatDoesNotExist()
        {
            // This is the exact scenario the old code got wrong: it claimed (via its non-nullable
            // return type) that this could never happen, then quietly returned null anyway.
            var result = await Svc<IProductServices>().GetById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ReturnsNull_ForAnIdThatDoesNotExist()
        {
            var dto = NewProductDto();
            dto.Id = Guid.NewGuid(); // an id that was never actually created

            var result = await Svc<IProductServices>().Update(dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_ReturnsNull_ForAnIdThatDoesNotExist()
        {
            var result = await Svc<IProductServices>().Delete(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Create_ThenGetById_ReturnsTheSameProduct()
        {
            // Arrange
            var dto = NewProductDto();

            // Act
            var created = await Svc<IProductServices>().Create(dto);
            var fetched = await Svc<IProductServices>().GetById(created.Id);

            // Assert
            Assert.NotNull(fetched);
            Assert.Equal(dto.Name, fetched!.Name);
            Assert.Equal(dto.Price, fetched.Price);
        }

        [Fact]
        public async Task Update_ChangesArePersisted()
        {
            // Arrange: create a product, then build an update DTO pointing at its real ID.
            var created = await Svc<IProductServices>().Create(NewProductDto());
            var updateDto = NewProductDto();
            updateDto.Id = created.Id;
            updateDto.Price = 19.99m;
            updateDto.Name = "Test Toode (muudetud)";

            // Act
            var updated = await Svc<IProductServices>().Update(updateDto);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal(19.99m, updated!.Price);
            Assert.Equal("Test Toode (muudetud)", updated.Name);
        }

        [Fact]
        public async Task Delete_RemovesTheProductFromTheDatabase()
        {
            // Arrange
            var created = await Svc<IProductServices>().Create(NewProductDto());

            // Act
            var deleted = await Svc<IProductServices>().Delete(created.Id);
            var fetchedAfterDelete = await Svc<IProductServices>().GetById(created.Id);

            // Assert
            Assert.NotNull(deleted);
            Assert.Null(fetchedAfterDelete); // it's really gone, not just marked as deleted
        }
    }
}
