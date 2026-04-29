namespace ElektriKalkulaator.Core.Domain
{
    // Groups products into categories like "Circuit Breakers", "Cables", "RCDs".
    public class ProductCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Populated by EF Core when you call .Include(c => c.Products)
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
