using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Tests that every BOM line reports the right unit of measure.
    //
    // Why this exists: the result table used to print "tk" (pieces) on every row, including cable.
    // A 40-metre cable run therefore read "40 tk", which looks like forty separate cables, and its
    // price read "1.20 €" as though that were the price of the whole run rather than one metre.
    // Anyone using the estimate to order materials would have been badly misled.
    public class BomUnitOfMeasureTests : TestBase
    {
        private static CalculatorInputDto Input(
            int rooms = 3, int lights = 8, int sockets = 6, bool stove = true) => new()
        {
            BuildingType = "korterelamu",
            RoomCount = rooms,
            LightCount = lights,
            SocketCount = sockets,
            HasElectricStove = stove
        };

        [Fact]
        public async Task CableLines_AreMeasuredInMetres()
        {
            var bom = await Svc<ICalculatorServices>().Calculate(Input());

            var cables = bom.Where(b => b.ProductName.Contains("kaabel")).ToList();

            Assert.NotEmpty(cables);
            Assert.All(cables, line => Assert.Equal("m", line.Unit));
        }

        [Fact]
        public async Task BreakerLines_AreCountedInPieces()
        {
            var bom = await Svc<ICalculatorServices>().Calculate(Input());

            var breakers = bom
                .Where(b => b.CircuitType is "lighting" or "socket" or "stove"
                            && !b.ProductName.Contains("kaabel"))
                .ToList();

            Assert.NotEmpty(breakers);
            Assert.All(breakers, line => Assert.Equal("tk", line.Unit));
        }

        [Fact]
        public async Task EnclosureAndRcd_AreCountedInPieces()
        {
            var bom = await Svc<ICalculatorServices>().Calculate(Input());

            Assert.Equal("tk", bom.Single(b => b.CircuitType == "panel").Unit);
            Assert.Equal("tk", bom.Single(b => b.CircuitType == "rcd").Unit);
        }

        [Fact]
        public async Task EveryLine_HasAUnitSet()
        {
            // A blank unit would render as "40 " with nothing after it.
            var bom = await Svc<ICalculatorServices>().Calculate(Input());

            Assert.All(bom, line => Assert.False(string.IsNullOrWhiteSpace(line.Unit)));
        }

        [Fact]
        public async Task OnlyKnownUnitsAreUsed()
        {
            // Guards against a future component type inventing a third unit that the view has no
            // wording for.
            var bom = await Svc<ICalculatorServices>().Calculate(Input());

            Assert.All(bom, line => Assert.Contains(line.Unit, new[] { "tk", "m" }));
        }

        [Fact]
        public async Task CablePricing_IsPerMetre_SoTheLineTotalScalesWithLength()
        {
            // 5 rooms x 8 m = 40 m per circuit, one lighting circuit for 8 lights.
            // The seeded 1.5 mm² cable costs 1.20 €/m, so the line must total 48.00 €, not 1.20 €.
            var bom = await Svc<ICalculatorServices>().Calculate(
                Input(rooms: 5, lights: 8, sockets: 0, stove: false));

            var cable = bom.Single(b => b.CircuitType == "lighting" && b.ProductName.Contains("kaabel"));

            Assert.Equal("m", cable.Unit);
            Assert.Equal(40, cable.Quantity);
            Assert.Equal(1.20m, cable.UnitPrice);
            Assert.Equal(48.00m, cable.TotalPrice);
        }
    }
}
