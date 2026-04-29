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

        public ProductsController(
            IProductServices productServices,
            ICategoryServices categoryServices)
        {
            _productServices = productServices;
            _categoryServices = categoryServices;
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
        public async Task<IActionResult> Create(ProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesIntoViewBag();
                return View(dto);
            }

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
        public async Task<IActionResult> Edit(Guid id, ProductDto dto)
        {
            if (!dto.Id.HasValue || id != dto.Id.Value)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesIntoViewBag(dto.CategoryId);
                return View(dto);
            }

            await _productServices.Update(dto);
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
    }
}
