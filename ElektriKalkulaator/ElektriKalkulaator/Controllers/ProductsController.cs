using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElektriKalkulaator.Controllers
{
    // Full CRUD for products plus category management.
    // Routes: /Products, /Products/Details, /Products/Create, /Products/Edit, /Products/Delete, /Products/Categories
    public class ProductsController : Controller
    {
        private readonly IProductServices _productServices;
        private readonly ICategoryServices _categoryServices;
        private readonly IWebHostEnvironment _env;

        public ProductsController(
            IProductServices productServices,
            ICategoryServices categoryServices,
            IWebHostEnvironment env)
        {
            _productServices = productServices;
            _categoryServices = categoryServices;
            _env = env;
        }

        // GET /Products — catalogue with optional category filter and name search
        [HttpGet]
        public async Task<IActionResult> Index(Guid? categoryId, string? searchTerm)
        {
            var categories = await _categoryServices.GetAll();

            var products = categoryId.HasValue
                ? await _productServices.GetByCategory(categoryId.Value)
                : await _productServices.GetAll();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Brand.ToLower().Contains(term));
            }

            ViewBag.Categories       = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchTerm       = searchTerm;

            return View(products);
        }

        // GET /Products/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productServices.GetById(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET /Products/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesIntoViewBag();
            return View(new ProductDto());
        }

        // POST /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto dto, IFormFile? imageFile)
        {
            // Check the uploaded file BEFORE anything else. If it's not acceptable we add the
            // reason to ModelState, which makes ASP.NET treat it exactly like any other failed
            // form validation: the page is redisplayed with the message next to the field.
            var imageError = ValidateImageFile(imageFile);
            if (imageError != null)
                ModelState.AddModelError(nameof(imageFile), imageError);

            if (!ModelState.IsValid)
            {
                await LoadCategoriesIntoViewBag();
                return View(dto);
            }

            dto.ImagePath = await SaveProductImage(imageFile);

            await _productServices.Create(dto);
            TempData["Success"] = $"Product '{dto.Name}' added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET /Products/Edit/{id} — map domain object to DTO for the form
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productServices.GetById(id);
            if (product == null) return NotFound();

            var dto = new ProductDto
            {
                Id                  = product.Id,
                CategoryId          = product.CategoryId,
                Name                = product.Name,
                Brand               = product.Brand,
                RatedCurrent        = product.RatedCurrent,
                Voltage             = product.Voltage,
                Price               = product.Price,
                StockQuantity       = product.StockQuantity,
                ImagePath           = product.ImagePath,
                WireCrossSectionMm2 = product.WireCrossSectionMm2,
                Description         = product.Description
            };

            await LoadCategoriesIntoViewBag(product.CategoryId);
            return View(dto);
        }

        // POST /Products/Edit/{id} — check URL id matches form id before saving
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductDto dto, IFormFile? imageFile)
        {
            if (!dto.Id.HasValue || id != dto.Id.Value)
                return BadRequest();

            // Same file check as Create — see the comment there.
            var imageError = ValidateImageFile(imageFile);
            if (imageError != null)
                ModelState.AddModelError(nameof(imageFile), imageError);

            if (!ModelState.IsValid)
            {
                await LoadCategoriesIntoViewBag(dto.CategoryId);
                return View(dto);
            }

            // A new file replaces the image; otherwise dto.ImagePath already holds the existing
            // path, round-tripped through a hidden field in the Edit form. Keep the old file's
            // path so we can delete it from disk once the new one is confirmed saved — otherwise
            // every re-upload leaves an orphaned file behind.
            var oldImagePath = dto.ImagePath;
            var newImagePath = await SaveProductImage(imageFile);
            if (newImagePath != null)
            {
                dto.ImagePath = newImagePath;
                DeleteProductImageFile(oldImagePath);
            }

            var updated = await _productServices.Update(dto);
            if (updated == null)
                return NotFound();

            TempData["Success"] = $"Product '{dto.Name}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET /Products/Delete/{id} — confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _productServices.GetById(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST /Products/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _productServices.Delete(id);
            if (product == null)
                return NotFound();

            DeleteProductImageFile(product.ImagePath);

            TempData["Success"] = $"Product '{product.Name}' deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET /Products/Categories — admin list
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryServices.GetAll();
            return View(categories);
        }

        // POST /Products/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Category name is required.";
                return RedirectToAction(nameof(Categories));
            }

            var category = new ElektriKalkulaator.Core.Domain.ProductCategory
            {
                Id          = Guid.NewGuid(),
                Name        = name,
                Description = description
            };

            await _categoryServices.Create(category);
            TempData["Success"] = $"Category '{name}' added successfully!";
            return RedirectToAction(nameof(Categories));
        }

        // Shared helper — builds the category SelectList for Create and Edit forms.
        private async Task LoadCategoriesIntoViewBag(Guid? selectedId = null)
        {
            var categories = await _categoryServices.GetAll();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }

        // Product images live in two separate folders, and the distinction matters:
        //
        //   wwwroot/images/products/  — the photos that ship WITH the app for the seeded demo
        //                               catalogue. These are source files, committed to git, and
        //                               several products share the same file (all five breakers
        //                               point at breaker.jpg).
        //   wwwroot/images/uploads/   — photos an admin uploads through the Create/Edit form.
        //                               These are user content, excluded from git, and each one
        //                               belongs to exactly one product.
        //
        // Keeping them apart is what lets the app safely delete an upload without ever touching
        // a shipped file that other products still rely on. See DeleteProductImageFile below.
        private const string UploadsFolderName = "uploads";
        private const string UploadsWebPath = "/images/uploads/";

        // There is no login/authorization on this controller (by design, for the thesis scope —
        // see PROJECT_ROADMAP.md), which means anyone who can reach /Products/Create or /Edit can
        // reach this upload path too. An extension allow-list and a size cap are the minimum
        // guardrails against someone using the form to drop arbitrary files on the server.
        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        // Checks whether an uploaded file is one we're willing to store.
        //
        // Returns null when the file is fine (including when there is no file at all — the image
        // is optional), or a message written for the person filling in the form when it isn't.
        // The message is in Estonian because every label on that form is in Estonian.
        //
        // Validation lives in its own method, separate from saving, so the controller can report
        // a problem on the form instead of the upload blowing up halfway through. Previously a
        // wrong file type threw an exception and the user got a blank 500 error page with no idea
        // what went wrong.
        private static string? ValidateImageFile(IFormFile? imageFile)
        {
            // No file chosen is perfectly valid — a product simply has no picture.
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var extension = Path.GetExtension(imageFile.FileName);
            if (!AllowedImageExtensions.Contains(extension))
                return $"Sobimatu failitüüp. Lubatud on: {string.Join(", ", AllowedImageExtensions)}.";

            if (imageFile.Length > MaxImageSizeBytes)
                return $"Pilt on liiga suur (suurim lubatud maht on {MaxImageSizeBytes / 1024 / 1024} MB).";

            return null;
        }

        // Saves an uploaded image into wwwroot/images/uploads. Anything under wwwroot is served
        // by the default static-file middleware, so no extra configuration is needed.
        // Returns the web-relative path to store in Product.ImagePath, or null if no file was sent.
        //
        // Assumes ValidateImageFile has already approved the file — always call that first.
        private async Task<string?> SaveProductImage(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var extension = Path.GetExtension(imageFile.FileName);
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", UploadsFolderName);
            Directory.CreateDirectory(uploadsFolder);

            // A fresh GUID filename means an upload can never collide with or overwrite another
            // product's image, and sidesteps the original (client-supplied, untrusted) filename
            // entirely except for its extension.
            var fileName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"{UploadsWebPath}{fileName}";
        }

        // Removes a previously-uploaded product image from disk — called when a product is
        // deleted, and when Edit replaces an existing image with a new upload.
        //
        // It deliberately deletes ONLY files under wwwroot/images/uploads. Two reasons:
        //   1. The seeded demo photos in wwwroot/images/products are shared — all five breakers
        //      point at the same breaker.jpg — so deleting one product must not remove an image
        //      four other products still display.
        //   2. The path comes from the database. Restricting deletion to one known folder means
        //      that even a malformed or hand-edited value (e.g. "../../appsettings.json") cannot
        //      make this method delete something outside the uploads directory.
        //
        // Safe to call with null/empty (a product that never had an image) or a path that's
        // already gone.
        private void DeleteProductImageFile(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            // Ignore anything that isn't an uploaded file — seeded images are shipped assets.
            if (!imagePath.StartsWith(UploadsWebPath, StringComparison.OrdinalIgnoreCase))
                return;

            var fileName = Path.GetFileName(imagePath);
            if (string.IsNullOrEmpty(fileName))
                return;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", UploadsFolderName);
            var fullPath = Path.Combine(uploadsFolder, fileName);

            // Fully qualified: Controller.File(...) shadows System.IO.File within this class.
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
