using System.ComponentModel.DataAnnotations;

namespace ElektriKalkulaator.Core.Dto
{
    // Form data coming in from the calculator page.
    // Keeps domain models clean — no IDs or timestamps here, those get added by the system.
    public class CalculatorInputDto
    {
        [Required(ErrorMessage = "Please select a building type")]
        public string BuildingType { get; set; } = "";

        [Range(1, 200, ErrorMessage = "Room count must be between 1 and 200")]
        public int RoomCount { get; set; }

        [Range(0, 500, ErrorMessage = "Socket count must be between 0 and 500")]
        public int SocketCount { get; set; }

        [Range(0, 500, ErrorMessage = "Light count must be between 0 and 500")]
        public int LightCount { get; set; }

        // Switches don't affect circuit calculation — stored for reference only
        public int SwitchCount { get; set; }

        // Adds a dedicated 6mm²/32A stove circuit when true
        public bool HasElectricStove { get; set; }

        // Optional — used for wire length estimates
        public int? FloorCount { get; set; }
        public decimal? TotalAreaM2 { get; set; }
    }
}
