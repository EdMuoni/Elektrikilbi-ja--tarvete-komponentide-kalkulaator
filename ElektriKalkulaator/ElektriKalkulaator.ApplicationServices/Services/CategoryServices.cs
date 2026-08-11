using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.ServiceInterface;
using ElektriKalkulaator.Data;
using Microsoft.EntityFrameworkCore;

namespace ElektriKalkulaator.ApplicationServices.Services
{
    // Manages product categories (Circuit Breakers, Cables, RCDs, etc.).
    // Admins can add/delete categories from /Products/Categories.
    public class CategoryServices : ICategoryServices
    {
        private readonly ElektriKalkulaatorContext _context;

        public CategoryServices(ElektriKalkulaatorContext context)
        {
            _context = context;
        }

        // Used in filter dropdowns and the Create/Edit product forms.
        public async Task<IEnumerable<ProductCategory>> GetAll()
        {
            return await _context.ProductCategories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // Returns null if not found — caller is responsible for checking.
        public async Task<ProductCategory?> GetById(Guid id)
        {
            return await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Auto-generates an ID if none was set before calling.
        public async Task<ProductCategory> Create(ProductCategory category)
        {
            if (category.Id == Guid.Empty)
                category.Id = Guid.NewGuid();

            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        // Deletes a category, but refuses if products still belong to it — removing it would
        // leave those products pointing at a category that no longer exists.
        //
        // The return value tells the caller which of the three things happened, so it can show a
        // helpful message instead of crashing. This matches how ProductServices reports a missing
        // record (by returning a value, not by throwing): a category the user asked to delete
        // being absent is an expected outcome, not a program error.
        //
        // NOTE: nothing calls this yet — there is no "delete category" button in the UI. It is
        // kept because the admin area planned in PROJECT_ROADMAP.md will need it.
        public async Task<CategoryDeleteResult> Delete(Guid id)
        {
            var category = await _context.ProductCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return CategoryDeleteResult.NotFound;

            if (category.Products.Any())
                return CategoryDeleteResult.StillHasProducts;

            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();
            return CategoryDeleteResult.Deleted;
        }
    }
}
