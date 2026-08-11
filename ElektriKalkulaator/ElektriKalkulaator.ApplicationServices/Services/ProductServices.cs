using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;
using ElektriKalkulaator.Data;
using Microsoft.EntityFrameworkCore;

namespace ElektriKalkulaator.ApplicationServices.Services
{
    // Standard CRUD service for products — same pattern as ShopTARge24.
    public class ProductServices : IProductServices
    {
        private readonly ElektriKalkulaatorContext _context;

        public ProductServices(ElektriKalkulaatorContext context)
        {
            _context = context;
        }

        // Used on the /Products catalogue page, sorted by category then name.
        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Category!.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        // Used when the user picks a category from the filter dropdown.
        public async Task<IEnumerable<Product>> GetByCategory(Guid categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.RatedCurrent)  // 10A → 16A → 32A
                .ToListAsync();
        }

        // Catalogue search used by the /Products page.
        //
        // The important detail is that the filters are added to the query BEFORE ToListAsync()
        // is called. Until that point nothing has run — EF Core is still building a description
        // of the query — so every filter becomes part of the SQL WHERE clause and the database
        // returns only the rows we actually want.
        //
        // The previous version fetched every product and then filtered the list in C#. That works
        // with ten products and wastes the whole table with ten thousand.
        public async Task<IEnumerable<Product>> Search(Guid? categoryId, string? searchTerm)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                // EF Core translates Contains into a SQL LIKE. SQL Server's default collation is
                // case-insensitive, so no ToLower() is needed — and leaving it out lets the
                // database use an index instead of transforming every row first.
                query = query.Where(p => p.Name.Contains(term) || p.Brand.Contains(term));
            }

            return await query
                .OrderBy(p => p.Category!.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        // Returns null if not found — the controller checks and returns NotFound().
        public async Task<Product?> GetById(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Maps the form DTO to a domain object and saves it.
        // Reloads the Category nav property so the caller has the full object.
        public async Task<Product> Create(ProductDto dto)
        {
            var product = new Product
            {
                Id                  = Guid.NewGuid(),
                CategoryId          = dto.CategoryId,
                Name                = dto.Name,
                Brand               = dto.Brand,
                RatedCurrent        = dto.RatedCurrent,
                Voltage             = dto.Voltage,
                Price               = dto.Price,
                StockQuantity       = dto.StockQuantity,
                ImagePath           = dto.ImagePath,
                WireCrossSectionMm2 = dto.WireCrossSectionMm2,
                Description         = dto.Description,
                CreatedAt           = DateTime.Now,
                ModifiedAt          = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Load the Category so it's available right after creating
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

            return product;
        }

        // EF Core tracks changes automatically — just update fields and save.
        // Returns null if the product doesn't exist — the controller checks and returns NotFound(),
        // same convention as GetById (a missing ID is an expected outcome, not an exceptional one).
        public async Task<Product?> Update(ProductDto dto)
        {
            if (!dto.Id.HasValue)
                throw new Exception("Product ID is missing.");

            var product = await _context.Products.FindAsync(dto.Id.Value);
            if (product == null)
                return null;

            product.CategoryId          = dto.CategoryId;
            product.Name                = dto.Name;
            product.Brand               = dto.Brand;
            product.RatedCurrent        = dto.RatedCurrent;
            product.Voltage             = dto.Voltage;
            product.Price               = dto.Price;
            product.StockQuantity       = dto.StockQuantity;
            product.ImagePath           = dto.ImagePath;
            product.WireCrossSectionMm2 = dto.WireCrossSectionMm2;
            product.Description         = dto.Description;
            product.ModifiedAt          = DateTime.Now;

            await _context.SaveChangesAsync();
            return product;
        }

        // Returns the deleted product so the controller can show "X was deleted" message.
        // Returns null if the product doesn't exist — same not-found convention as GetById/Update.
        public async Task<Product?> Delete(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return product;
        }
    }
}
