using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;
using ElektriKalkulaator.Data;
using Microsoft.EntityFrameworkCore;

namespace ElektriKalkulaator.ApplicationServices.Services
{
    // The core of the app — applies EVS-HD 60364 rules to the user's input
    // and returns a Bill of Materials with matching products from the database.
    public class CalculatorServices : ICalculatorServices
    {
        private readonly ElektriKalkulaatorContext _context;

        public CalculatorServices(ElektriKalkulaatorContext context)
        {
            _context = context;
        }

        // Calculates the required components based on EVS-HD 60364:
        //   Lighting:  1 circuit per 8 lights  — 1.5mm² / 10A
        //   Sockets:   1 circuit per 6 sockets — 2.5mm² / 16A
        //   Stove:     1 dedicated circuit      — 6.0mm² / 32A
        public async Task<List<BOMItemDto>> Calculate(CalculatorInputDto input)
        {
            // Load rules for the selected building type from the database
            var rules = await _context.CalculationRules
                .Where(r => r.BuildingType == input.BuildingType)
                .ToListAsync();

            if (!rules.Any())
                return new List<BOMItemDto>();

            // How many circuits are needed per type
            int lightingCircuits = (int)Math.Ceiling(input.LightCount / 8.0);
            int socketCircuits   = (int)Math.Ceiling(input.SocketCount / 6.0);
            int stoveCircuits    = input.HasElectricStove ? 1 : 0;

            // Rough wire estimate: 8 metres of cable per room per circuit
            int wireLengthPerCircuit = input.RoomCount * 8;

            var bom = new List<BOMItemDto>();

            foreach (var rule in rules)
            {
                int circuitCount = rule.CircuitType switch
                {
                    "lighting" => lightingCircuits,
                    "socket"   => socketCircuits,
                    "stove"    => stoveCircuits,
                    _          => 0
                };

                if (circuitCount == 0) continue;

                // Find the cheapest in-stock breaker matching the required rating
                var breaker = await _context.Products
                    .Include(p => p.Category)
                    .Where(p =>
                        p.RatedCurrent == rule.BreakerAmperes &&
                        p.StockQuantity > 0 &&
                        p.Category!.Name == "Kaitselülitid")
                    .OrderBy(p => p.Price)
                    .FirstOrDefaultAsync();

                if (breaker != null)
                {
                    bom.Add(new BOMItemDto
                    {
                        ProductId           = breaker.Id,
                        ProductName         = breaker.Name,
                        Brand               = breaker.Brand,
                        Quantity            = circuitCount,
                        UnitPrice           = breaker.Price,
                        TotalPrice          = breaker.Price * circuitCount,
                        CircuitType         = rule.CircuitType,
                        WireCrossSectionMm2 = rule.WireCrossSectionMm2
                    });
                }

                // Find the cheapest in-stock cable matching the required cross-section
                var wire = await _context.Products
                    .Include(p => p.Category)
                    .Where(p =>
                        p.WireCrossSectionMm2 == rule.WireCrossSectionMm2 &&
                        p.StockQuantity > 0 &&
                        p.Category!.Name == "Juhtmed")
                    .OrderBy(p => p.Price)
                    .FirstOrDefaultAsync();

                // Wire is sold by the metre — quantity = circuits × metres per circuit
                int wireMeters = circuitCount * wireLengthPerCircuit;

                if (wire != null && wireMeters > 0)
                {
                    bom.Add(new BOMItemDto
                    {
                        ProductId           = wire.Id,
                        ProductName         = wire.Name,
                        Brand               = wire.Brand,
                        Quantity            = wireMeters,
                        UnitPrice           = wire.Price,
                        TotalPrice          = wire.Price * wireMeters,
                        CircuitType         = rule.CircuitType,
                        WireCrossSectionMm2 = rule.WireCrossSectionMm2
                    });
                }
            }

            // Add the enclosure (one per installation)
            var panelBox = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0 && p.Category!.Name == "Kilbi korpused")
                .OrderBy(p => p.Price)
                .FirstOrDefaultAsync();

            if (panelBox != null)
            {
                bom.Add(new BOMItemDto
                {
                    ProductId   = panelBox.Id,
                    ProductName = panelBox.Name,
                    Brand       = panelBox.Brand,
                    Quantity    = 1,
                    UnitPrice   = panelBox.Price,
                    TotalPrice  = panelBox.Price,
                    CircuitType = "panel"
                });
            }

            // Add the RCD — required by EVS-HD 60364 for fault protection
            var rcd = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0 && p.Category!.Name == "RCD / Rikkevoolukaitsmeid")
                .OrderBy(p => p.Price)
                .FirstOrDefaultAsync();

            if (rcd != null)
            {
                bom.Add(new BOMItemDto
                {
                    ProductId   = rcd.Id,
                    ProductName = rcd.Name,
                    Brand       = rcd.Brand,
                    Quantity    = 1,
                    UnitPrice   = rcd.Price,
                    TotalPrice  = rcd.Price,
                    CircuitType = "rcd"
                });
            }

            return bom;
        }

        // Persists the calculation to three tables:
        //   PowerboxCalculation  — session header (status, total cost)
        //   PowerboxRequirements — what the user entered
        //   PowerboxComponents   — each BOM line item
        public async Task<PowerboxCalculation> SaveCalculation(
            CalculatorInputDto input,
            List<BOMItemDto> bom)
        {
            var calculation = new PowerboxCalculation
            {
                Id           = Guid.NewGuid(),
                CreatedAt    = DateTime.Now,
                ModifiedAt   = DateTime.Now,
                Status       = "completed",
                TotalCost    = bom.Sum(b => b.TotalPrice),
                RulesApplied = string.Join(", ", bom
                    .Where(b => b.CircuitType != null)
                    .Select(b => b.CircuitType!)
                    .Distinct())
            };

            _context.PowerboxCalculations.Add(calculation);

            var requirements = new PowerboxRequirements
            {
                Id               = Guid.NewGuid(),
                CalculationId    = calculation.Id,
                BuildingType     = input.BuildingType,
                RoomCount        = input.RoomCount,
                SocketCount      = input.SocketCount,
                LightCount       = input.LightCount,
                SwitchCount      = input.SwitchCount,
                HasElectricStove = input.HasElectricStove,
                FloorCount       = input.FloorCount,
                TotalAreaM2      = input.TotalAreaM2,
                CreatedAt        = DateTime.Now,
                ModifiedAt       = DateTime.Now
            };

            _context.PowerboxRequirements.Add(requirements);

            foreach (var item in bom)
            {
                _context.PowerboxComponents.Add(new PowerboxComponents
                {
                    Id                  = Guid.NewGuid(),
                    CalculationId       = calculation.Id,
                    ProductId           = item.ProductId,
                    Quantity            = item.Quantity,
                    UnitPrice           = item.UnitPrice,
                    TotalPrice          = item.TotalPrice,
                    CircuitType         = item.CircuitType,
                    WireCrossSectionMm2 = item.WireCrossSectionMm2,
                    CreatedAt           = DateTime.Now,
                    ModifiedAt          = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return calculation;
        }

        // Returns the 50 most recent calculations for the history page.
        public async Task<IEnumerable<PowerboxCalculation>> GetHistory()
        {
            return await _context.PowerboxCalculations
                .Include(c => c.Requirements)
                .Include(c => c.Components)
                    .ThenInclude(comp => comp.Product)
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .ToListAsync();
        }
    }
}
