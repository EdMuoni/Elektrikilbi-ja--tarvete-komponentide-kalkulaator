namespace ElektriKalkulaator.Core.Domain
{
    // One rule from the EVS-HD 60364 standard, e.g. "apartment lighting: 1.5mm², 10A".
    // Rules live in the database — if the standard changes, update the data, not the code.
    public class CalculationRule
    {
        public Guid Id { get; set; }

        public string RuleName { get; set; } = "";

        // "korterelamu" | "eramu" | "ärihoone"
        public string BuildingType { get; set; } = "";

        // Required wire cross-section: 1.5 = lighting, 2.5 = sockets, 6.0 = stove
        public decimal WireCrossSectionMm2 { get; set; }

        // Required breaker rating: 10A = lighting, 16A = sockets, 32A = stove
        public int BreakerAmperes { get; set; }

        // "lighting" | "socket" | "stove"
        public string CircuitType { get; set; } = "";

        // Exact EVS-HD 60364 clause this rule implements, e.g. "EVS-HD 60364-4-41".
        // Left unset in seed data until verified against the actual standard text — do not
        // guess a clause number here, a wrong citation is worse than a missing one.
        public string? EvsReference { get; set; }

        public int RoomsFrom { get; set; }
        public int RoomsTo { get; set; }  // 999 = no upper limit

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
