using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Boundary and edge-case tests for the calculator.
    //
    // CalculatorServicesTests covers the normal paths. This class deliberately pushes at the
    // edges — zero, one, exact multiples, the maximum the form allows — because that is where
    // arithmetic mistakes live. "1 circuit per 8 lights" is easy to write and easy to get wrong by
    // one in either direction.
    public class CalculatorEdgeCaseTests : TestBase
    {
        private static CalculatorInputDto Input(
            string buildingType = "korterelamu",
            int rooms = 3, int lights = 0, int sockets = 0, bool stove = false) => new()
        {
            BuildingType = buildingType,
            RoomCount = rooms,
            LightCount = lights,
            SocketCount = sockets,
            HasElectricStove = stove
        };

        private async Task<int> BreakerQuantityFor(CalculatorInputDto input, string circuitType)
        {
            var bom = await Svc<ICalculatorServices>().Calculate(input);
            return bom
                .Where(b => b.CircuitType == circuitType && !b.ProductName.Contains("kaabel"))
                .Sum(b => b.Quantity);
        }

        // Lighting: one circuit per 8 lights. The boundaries either side of each multiple are
        // where an off-by-one would show up.
        [Theory]
        [InlineData(0, 0)]    // no lights at all -> no lighting circuit
        [InlineData(1, 1)]    // a single light still needs a whole circuit
        [InlineData(7, 1)]
        [InlineData(8, 1)]    // exactly one full circuit, must NOT round up to 2
        [InlineData(9, 2)]    // one over -> a second circuit
        [InlineData(16, 2)]   // exactly two
        [InlineData(17, 3)]
        [InlineData(500, 63)] // the form's maximum: ceil(500/8) = 62.5 -> 63
        public async Task LightingCircuits_AreOnePerEightLights(int lights, int expectedCircuits)
        {
            var quantity = await BreakerQuantityFor(Input(lights: lights), "lighting");
            Assert.Equal(expectedCircuits, quantity);
        }

        // Sockets: one circuit per 6 sockets.
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 1)]
        [InlineData(6, 1)]    // exactly one
        [InlineData(7, 2)]
        [InlineData(12, 2)]
        [InlineData(500, 84)] // ceil(500/6) = 83.33 -> 84
        public async Task SocketCircuits_AreOnePerSixSockets(int sockets, int expectedCircuits)
        {
            var quantity = await BreakerQuantityFor(Input(sockets: sockets), "socket");
            Assert.Equal(expectedCircuits, quantity);
        }

        // All three building types offered in the form must produce a usable result.
        [Theory]
        [InlineData("korterelamu")]
        [InlineData("eramu")]
        [InlineData("ärihoone")]
        public async Task EveryBuildingType_ProducesABom(string buildingType)
        {
            var bom = await Svc<ICalculatorServices>().Calculate(
                Input(buildingType, lights: 8, sockets: 6));

            Assert.NotEmpty(bom);
            Assert.Contains(bom, b => b.CircuitType == "panel");
            Assert.Contains(bom, b => b.CircuitType == "rcd");
        }

        [Fact]
        public async Task CommercialBuildings_HaveNoStoveRule_SoNoStoveCircuitIsAdded()
        {
            // Only korterelamu and eramu have a "stove" rule seeded. Ticking the stove box for a
            // commercial building must therefore add nothing rather than crash or invent a rule.
            var bom = await Svc<ICalculatorServices>().Calculate(
                Input("ärihoone", lights: 8, stove: true));

            Assert.DoesNotContain(bom, b => b.CircuitType == "stove");
        }

        [Fact]
        public async Task BuildingTypeIsCaseSensitive_WhichIsWorthKnowing()
        {
            // Documents real behaviour rather than asserting it is desirable: rules are matched by
            // exact string, so "Korterelamu" does not match "korterelamu" and the result is empty.
            // The form only ever submits lowercase values, so this is not currently a live bug —
            // but anyone adding a new input path needs to know.
            var bom = await Svc<ICalculatorServices>().Calculate(Input("Korterelamu", lights: 8));

            Assert.Empty(bom);
        }

        [Fact]
        public async Task EmptyBuildingType_ReturnsEmptyRatherThanThrowing()
        {
            var bom = await Svc<ICalculatorServices>().Calculate(Input(""));
            Assert.Empty(bom);
        }

        [Fact]
        public async Task TotalCost_EqualsTheSumOfEveryLine()
        {
            // The displayed total must match the lines above it; a mismatch here is the kind of
            // thing a user notices immediately and trusts the tool less for.
            var input = Input(rooms: 4, lights: 10, sockets: 8, stove: true);

            var bom = await Svc<ICalculatorServices>().Calculate(input);
            var saved = await Svc<ICalculatorServices>().SaveCalculation(input, bom);

            Assert.Equal(bom.Sum(b => b.TotalPrice), saved.TotalCost);
        }

        [Fact]
        public async Task EveryLine_HasTotalPriceEqualToQuantityTimesUnitPrice()
        {
            var bom = await Svc<ICalculatorServices>().Calculate(
                Input(rooms: 5, lights: 20, sockets: 15, stove: true));

            Assert.All(bom, line =>
                Assert.Equal(line.UnitPrice * line.Quantity, line.TotalPrice));
        }

        [Fact]
        public async Task EveryLine_ReferencesARealProduct()
        {
            // ProductId is a foreign key when the BOM is saved, so a line pointing at nothing
            // would fail at save time rather than here — better to catch it immediately.
            var bom = await Svc<ICalculatorServices>().Calculate(
                Input(lights: 8, sockets: 6, stove: true));

            Assert.All(bom, line => Assert.True(
                Context.Products.Any(p => p.Id == line.ProductId),
                $"BOM line '{line.ProductName}' points at a product that does not exist."));
        }

        [Fact]
        public async Task OneRoom_StillProducesCable()
        {
            // Cable is estimated as RoomCount x 8 metres per circuit, so a one-room flat is the
            // smallest case. It must still order some cable rather than zero.
            var bom = await Svc<ICalculatorServices>().Calculate(Input(rooms: 1, lights: 8));

            var cable = bom.Single(b => b.CircuitType == "lighting" && b.ProductName.Contains("kaabel"));
            Assert.Equal(8, cable.Quantity);
        }

        [Fact]
        public async Task History_ReturnsMostRecentFirst()
        {
            var services = Svc<ICalculatorServices>();

            var first = Input(rooms: 1, lights: 8);
            await services.SaveCalculation(first, await services.Calculate(first));

            var second = Input(rooms: 9, lights: 8);
            await services.SaveCalculation(second, await services.Calculate(second));

            var history = (await services.GetHistory()).ToList();

            Assert.Equal(2, history.Count);
            Assert.Equal(9, history.First().Requirements!.RoomCount); // newest first
        }

        [Fact]
        public async Task History_ReturnsAtMostFiftyEntries()
        {
            var services = Svc<ICalculatorServices>();
            var input = Input(lights: 8);

            for (var i = 0; i < 55; i++)
                await services.SaveCalculation(input, await services.Calculate(input));

            Assert.Equal(50, (await services.GetHistory()).Count());
        }
    }
}
