namespace ElektriKalkulaator.Core.Domain
{
    // Created every time the user hits "Calculate" — links the input form to the BOM output.
    public class PowerboxCalculation
    {
        public Guid Id { get; set; }

        // Null for guest sessions (no login required in this app)
        public Guid? UserId { get; set; }

        // "processing" → "completed" | "error"
        public string Status { get; set; } = "completed";

        // Comma-separated circuit types that were applied, e.g. "lighting, socket, stove"
        public string? RulesApplied { get; set; }

        public decimal TotalCost { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // One-to-one: the input form data
        public PowerboxRequirements? Requirements { get; set; }

        // One-to-many: the resulting BOM rows
        public ICollection<PowerboxComponents>? Components { get; set; }
    }
}
