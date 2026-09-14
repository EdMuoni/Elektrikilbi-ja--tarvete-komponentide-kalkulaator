using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Tests covering what happens to Product.ImagePath as a product is edited.
    //
    // These exist because of a bug found on 2026-08-11: the Edit action deleted the previous image
    // file from disk BEFORE confirming the database update had succeeded, and it decided which
    // file to delete from a hidden form field the browser controls rather than from the database.
    //
    // The file-deletion itself lives in the controller and touches the filesystem, so it is
    // covered by scripts/security-check.sh and manual testing. What IS testable here — and what
    // the bug actually depended on — is that the stored path behaves correctly.
    public class ProductImageLifecycleTests : TestBase
    {
        private Guid AnyCategoryId => Context.ProductCategories.First().Id;

        private ProductDto NewProductDto(string? imagePath = null) => new()
        {
            CategoryId = AnyCategoryId,
            Name = "Pildi test",
            Brand = "TestBrand",
            RatedCurrent = 16,
            Voltage = 230,
            Price = 5.00m,
            StockQuantity = 1,
            ImagePath = imagePath
        };

        [Fact]
        public async Task Create_StoresTheImagePathItWasGiven()
        {
            var created = await Svc<IProductServices>()
                .Create(NewProductDto("/images/uploads/first.jpg"));

            Assert.Equal("/images/uploads/first.jpg", created.ImagePath);
        }

        [Fact]
        public async Task Create_AllowsAProductWithNoImage()
        {
            // The image is optional, so a null path must survive as null rather than becoming "".
            var created = await Svc<IProductServices>().Create(NewProductDto(null));

            Assert.Null(created.ImagePath);
        }

        [Fact]
        public async Task Update_ReplacesTheStoredPath_WhenANewImageIsSupplied()
        {
            var created = await Svc<IProductServices>()
                .Create(NewProductDto("/images/uploads/old.jpg"));

            var dto = NewProductDto("/images/uploads/new.jpg");
            dto.Id = created.Id;

            var updated = await Svc<IProductServices>().Update(dto);

            Assert.NotNull(updated);
            Assert.Equal("/images/uploads/new.jpg", updated!.ImagePath);
        }

        [Fact]
        public async Task Update_CanClearTheImage()
        {
            var created = await Svc<IProductServices>()
                .Create(NewProductDto("/images/uploads/old.jpg"));

            var dto = NewProductDto(null);
            dto.Id = created.Id;

            var updated = await Svc<IProductServices>().Update(dto);

            Assert.NotNull(updated);
            Assert.Null(updated!.ImagePath);
        }

        // The heart of the fixed bug. The controller now reads the current image path from the
        // database before deciding what to delete, instead of trusting the hidden form field.
        // This test pins the behaviour that makes that possible: the stored path is always
        // retrievable and is the authoritative value.
        [Fact]
        public async Task StoredImagePath_IsReadableFromTheDatabase_NotOnlyFromTheSubmittedForm()
        {
            var created = await Svc<IProductServices>()
                .Create(NewProductDto("/images/uploads/authoritative.jpg"));

            var fetched = await Svc<IProductServices>().GetById(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal("/images/uploads/authoritative.jpg", fetched!.ImagePath);
        }

        [Fact]
        public async Task Update_OfAProductThatWasDeleted_ReturnsNullAndChangesNothing()
        {
            // This is the failure path the old code handled badly: it had already deleted the
            // image file by the time it discovered the product was gone.
            var created = await Svc<IProductServices>()
                .Create(NewProductDto("/images/uploads/doomed.jpg"));

            await Svc<IProductServices>().Delete(created.Id);

            var dto = NewProductDto("/images/uploads/replacement.jpg");
            dto.Id = created.Id;

            var updated = await Svc<IProductServices>().Update(dto);

            Assert.Null(updated);
            Assert.Null(await Svc<IProductServices>().GetById(created.Id));
        }

        [Fact]
        public async Task SeededProducts_ShareImageFiles()
        {
            // Five breakers all point at the same file. This is why deleting one product must
            // never delete the file the others still use — see DeleteProductImageFile, which
            // refuses to touch anything outside the uploads folder.
            var all = await Svc<IProductServices>().GetAll();

            var breakerImages = all
                .Where(p => p.ImagePath == "/images/products/breaker.jpg")
                .ToList();

            Assert.Equal(5, breakerImages.Count);
        }
    }
}
