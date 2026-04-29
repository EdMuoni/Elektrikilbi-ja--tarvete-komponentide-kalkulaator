namespace ElektriKalkulaator.Core.Dto
{
    // One row in the Bill of Materials output — passed from the service to the view.
    public class BOMItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string Brand { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // Used for colour-coding rows in the table (lighting / socket / stove / rcd / panel)
        public string? CircuitType { get; set; }

        // Wire cross-section shown as a badge: 1.5 mm², 2.5 mm², 6.0 mm²
        public decimal? WireCrossSectionMm2 { get; set; }
    }
}
