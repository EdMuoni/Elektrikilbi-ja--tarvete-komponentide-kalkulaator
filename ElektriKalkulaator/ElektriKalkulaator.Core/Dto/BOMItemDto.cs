namespace ElektriKalkulaator.Core.Dto
{
    // One row in the Bill of Materials output — passed from the service to the view.
    public class BOMItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string Brand { get; set; } = "";

        // Path to the product photograph, or null when the product has no image.
        // Carried on the BOM row so the results table can show what each part LOOKS like.
        // A row reading "ABB S201-B32" means nothing to someone who is not an electrician;
        // a picture of a breaker does. It is the same reason the catalogue is photo-led.
        public string? ImagePath { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // What one unit of Quantity actually is: "tk" (pieces) for breakers, RCDs and enclosures,
        // "m" (metres) for cable.
        //
        // This matters more than it looks. Cable is calculated and priced by the metre, but the
        // result table used to label every row "tk", so a 40-metre cable run was displayed as
        // "40 tk" — reading as forty separate cables. Same for the price: 1.20 €/m was shown as if
        // it were 1.20 € per piece.
        public string Unit { get; set; } = "tk";

        // Used for colour-coding rows in the table (lighting / socket / stove / rcd / panel)
        public string? CircuitType { get; set; }

        // Wire cross-section shown as a badge: 1.5 mm², 2.5 mm², 6.0 mm²
        public decimal? WireCrossSectionMm2 { get; set; }
    }
}
