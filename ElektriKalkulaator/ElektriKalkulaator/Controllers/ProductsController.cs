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

        // There is no login/authorization on this controller (by design, for the thesis scope —
        // see PROJECT_ROADMAP.md), which means anyone who can reach /Products/Create or /Edit can
        // reach this upload path too. An extension allow-list and a size cap are the minimum
        // guardrails against someone using the form to drop arbitrary files on the server.
        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        // Saves an uploaded image straight into wwwroot/images/products (no extra static-file
        // middleware needed, unlike storing outside wwwroot). Returns the web-relative path to
        // store in Product.ImagePath, or null if no file was submitted.
        private async Task<string?> SaveProductImage(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var extension = Path.GetExtension(imageFile.FileName);
            if (!AllowedImageExtensions.Contains(extension))
                throw new InvalidOperationException(
                    $"Unsupported image type '{extension}'. Allowed: {string.Join(", ", AllowedImageExtensions)}");

            if (imageFile.Length > MaxImageSizeBytes)
                throw new InvalidOperationException("Image is too large (max 5 MB).");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
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

            return $"/images/products/{fileName}";
        }

        // Removes a previously-saved product image from disk — called when a product is deleted,
        // and when Edit replaces an existing image with a new upload. Safe to call with null/empty
        // (e.g. a product that never had an image) or a path that's already gone.
        private void DeleteProductImageFile(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            var relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            // Fully qualified: Controller.File(...) shadows System.IO.File within this class.
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
