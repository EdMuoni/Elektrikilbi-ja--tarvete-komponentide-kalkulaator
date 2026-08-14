using ElektriKalkulaator.Controllers;
using Microsoft.AspNetCore.Http;

namespace ElektriKalkulaator.Tests
{
    // Tests for the checks applied to an uploaded product image.
    //
    // These exist because the upload form has real consequences: it writes a file to the server's
    // disk. The checks were added after a review found that a text file renamed to ".jpg" was
    // accepted. These tests make sure that stays fixed.
    //
    // No database is needed here, so this class does NOT inherit TestBase — the validation is pure
    // logic operating on the uploaded file alone.
    public class ImageUploadValidationTests
    {
        // Builds a fake uploaded file from raw bytes, so a test can control exactly what the
        // "file" contains without touching the disk. This is what IFormFile looks like to the
        // controller when a real browser uploads something.
        private static IFormFile FakeUpload(string fileName, byte[] content)
        {
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, content.Length, "imageFile", fileName);
        }

        // Valid file signatures ("magic bytes") for each format we accept, padded to 12 bytes so
        // they are long enough to be read.
        private static byte[] JpegBytes() =>
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static byte[] PngBytes() =>
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };

        private static byte[] GifBytes() =>
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0, 0, 0, 0, 0, 0 };

        private static byte[] WebpBytes() =>
            new byte[] { 0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50 };

        [Fact]
        public void NoFileChosen_IsAccepted_BecauseTheImageIsOptional()
        {
            // A product without a picture is perfectly valid, so "no file" must not be an error.
            Assert.Null(ProductsController.ValidateImageFile(null));
        }

        [Fact]
        public void EmptyFile_IsTreatedAsNoFile()
        {
            var empty = FakeUpload("photo.jpg", Array.Empty<byte>());
            Assert.Null(ProductsController.ValidateImageFile(empty));
        }

        [Theory]
        [InlineData("photo.jpg")]
        [InlineData("photo.jpeg")]
        [InlineData("PHOTO.JPG")] // extension check must be case-insensitive
        public void RealJpeg_IsAccepted(string fileName)
        {
            var file = FakeUpload(fileName, JpegBytes());
            Assert.Null(ProductsController.ValidateImageFile(file));
        }

        [Fact]
        public void RealPng_IsAccepted()
        {
            Assert.Null(ProductsController.ValidateImageFile(FakeUpload("photo.png", PngBytes())));
        }

        [Fact]
        public void RealGif_IsAccepted()
        {
            Assert.Null(ProductsController.ValidateImageFile(FakeUpload("photo.gif", GifBytes())));
        }

        [Fact]
        public void RealWebp_IsAccepted()
        {
            Assert.Null(ProductsController.ValidateImageFile(FakeUpload("photo.webp", WebpBytes())));
        }

        [Theory]
        [InlineData("notes.txt")]
        [InlineData("program.exe")]
        [InlineData("script.js")]
        [InlineData("page.html")]
        public void DisallowedExtension_IsRejected(string fileName)
        {
            // Even with genuine JPEG content, an extension we don't allow is refused.
            var file = FakeUpload(fileName, JpegBytes());

            var error = ProductsController.ValidateImageFile(file);

            Assert.NotNull(error);
            Assert.Contains("Sobimatu failitüüp", error);
        }

        [Fact]
        public void FileLargerThanFiveMegabytes_IsRejected()
        {
            // 6 MB of valid JPEG content — the size limit must still refuse it.
            var big = new byte[6 * 1024 * 1024];
            JpegBytes().CopyTo(big, 0);

            var error = ProductsController.ValidateImageFile(FakeUpload("huge.jpg", big));

            Assert.NotNull(error);
            Assert.Contains("liiga suur", error);
        }

        // THE IMPORTANT ONE. This is the exact attack the review found: give a non-image file an
        // image extension and it used to be written to the server's disk.
        [Fact]
        public void ExecutableRenamedAsJpg_IsRejected()
        {
            // 0x4D 0x5A is "MZ", the signature every Windows .exe starts with.
            var exe = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0, 0, 0, 0x04, 0, 0, 0 };

            var error = ProductsController.ValidateImageFile(FakeUpload("payload.jpg", exe));

            Assert.NotNull(error);
            Assert.Contains("ei ole korrektne pildifail", error);
        }

        [Fact]
        public void PlainTextRenamedAsPng_IsRejected()
        {
            var text = System.Text.Encoding.ASCII.GetBytes("this is not an image at all");

            var error = ProductsController.ValidateImageFile(FakeUpload("fake.png", text));

            Assert.NotNull(error);
            Assert.Contains("ei ole korrektne pildifail", error);
        }

        [Fact]
        public void FileTooShortToContainASignature_IsRejected()
        {
            // Fewer than 12 bytes cannot contain any of the signatures we check.
            var tiny = new byte[] { 0xFF, 0xD8, 0xFF };

            var error = ProductsController.ValidateImageFile(FakeUpload("tiny.jpg", tiny));

            Assert.NotNull(error);
            Assert.Contains("ei ole korrektne pildifail", error);
        }

        [Fact]
        public void SignatureCheck_ReadsContentNotFileName()
        {
            // Directly proves the signature check ignores the name entirely: identical bytes give
            // the same answer whatever the file is called.
            var jpeg = JpegBytes();

            Assert.True(ProductsController.HasValidImageSignature(FakeUpload("a.jpg", jpeg)));
            Assert.True(ProductsController.HasValidImageSignature(FakeUpload("b.png", jpeg)));
            Assert.False(ProductsController.HasValidImageSignature(
                FakeUpload("c.jpg", System.Text.Encoding.ASCII.GetBytes("............"))));
        }
    }
}
