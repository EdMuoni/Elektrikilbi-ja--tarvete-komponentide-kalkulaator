namespace ElektriKalkulaator.Core.Domain
{
    // One BOM line item, e.g. "3× ABB B10A, €9.20 each = €27.60 total".
    // Unit price is stored separately because product prices can change later.
    public class PowerboxComponents
    {
        public Guid Id { get; set; }
        public Guid CalculationId { get; set; }  // FK → PowerboxCalculation
        public Guid ProductId { get; set; }       // FK → Product

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }   // = Quantity × UnitPrice

        public string? CircuitType { get; set; }
        public decimal? WireCrossSectionMm2 { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public PowerboxCalculation? Calculation { get; set; }
        public Product? Product { get; set; }
    }
}
