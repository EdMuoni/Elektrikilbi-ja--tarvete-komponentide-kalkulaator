namespace ElektriKalkulaator.Core.Domain
{
    // A single electrical component in the catalogue, e.g. "ABB S201-B16 16A breaker".
    public class Product
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }  // FK → ProductCategory

        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";

        // Used by the calculator to match the right breaker (10A / 16A / 32A)
        public decimal RatedCurrent { get; set; }
        public decimal Voltage { get; set; }

        // Always decimal for money — float/double causes rounding issues
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
        public string? ImagePath { get; set; }

        // Only set for cables: 1.5 / 2.5 / 6.0 mm²
        public decimal? WireCrossSectionMm2 { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // EF Core navigation — loaded on demand with .Include()
        public ProductCategory? Category { get; set; }
        public ICollection<PowerboxComponents>? PowerboxComponents { get; set; }
    }
}
