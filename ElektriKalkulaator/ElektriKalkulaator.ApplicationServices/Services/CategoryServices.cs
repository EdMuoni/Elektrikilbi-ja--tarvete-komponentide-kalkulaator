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
        public async Task<ProductCategory> GetById(Guid id)
        {
            return (await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id))!;
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

        // Refuses to delete if products still belong to this category (FK protection).
        public async Task<ProductCategory> Delete(Guid id)
        {
            var category = await _context.ProductCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new Exception($"Category with ID {id} was not found.");

            if (category.Products.Any())
            {
                throw new Exception(
                    $"Cannot delete '{category.Name}' — it still has {category.Products.Count} product(s). " +
                    "Move or delete those products first."
                );
            }

            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();
            return category;
        }
    }
}
