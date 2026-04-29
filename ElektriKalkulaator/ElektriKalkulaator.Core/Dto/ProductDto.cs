using System.ComponentModel.DataAnnotations;

namespace ElektriKalkulaator.Core.Dto
{
    // Form data for Create and Edit product pages.
    // No navigation properties or timestamps — the service layer handles those.
    public class ProductDto
    {
        // Null on Create (generated in the service), set on Edit (comes from the URL)
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Brand is required")]
        public string Brand { get; set; } = "";

        // 0 means not applicable (e.g. enclosures have no rated current)
        public decimal RatedCurrent { get; set; }
        public decimal Voltage { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 99999, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? ImagePath { get; set; }
        public decimal? WireCrossSectionMm2 { get; set; }
        public string? Description { get; set; }
    }
}
