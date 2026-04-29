namespace ElektriKalkulaator.Core.Domain
{
    // Snapshot of what the user entered — saved so the history page can show past inputs.
    public class PowerboxRequirements
    {
        public Guid Id { get; set; }
        public Guid CalculationId { get; set; }

        // "korterelamu" | "eramu" | "ärihoone"
        public string BuildingType { get; set; } = "";

        public int RoomCount { get; set; }

        // Every 6 sockets → one 2.5mm²/16A circuit
        public int SocketCount { get; set; }

        // Every 8 lights → one 1.5mm²/10A circuit
        public int LightCount { get; set; }

        // Informational only — doesn't affect circuit count
        public int SwitchCount { get; set; }

        // Adds a dedicated 6mm²/32A circuit when true
        public bool HasElectricStove { get; set; }

        public int? FloorCount { get; set; }
        public decimal? TotalAreaM2 { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public PowerboxCalculation? Calculation { get; set; }
    }
}
