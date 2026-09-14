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

        // One-to-many: the resulting BOM rows.
        //
        // Not nullable, and initialised to an empty list. A calculation always HAS components —
        // possibly none, which an empty list expresses perfectly well. Declaring it nullable said
        // "this list might not exist", which is a different and less useful idea, and it forced
        // every caller to null-check something that is never really null.
        //
        // It also produced a real compiler warning: EF Core's ThenInclude expects a non-nullable
        // collection, so `.Include(c => c.Components).ThenInclude(...)` in GetHistory raised
        // CS8620 on every build. This matches how ProductCategory.Products is already declared.
        public ICollection<PowerboxComponents> Components { get; set; } = new List<PowerboxComponents>();
    }
}
