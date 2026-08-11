using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Tests for CalculatorServices.Calculate() — the single most important piece of logic in
    // the whole thesis project. It's pure, deterministic arithmetic over data already sitting in
    // the database (the seeded CalculationRule rows encoding the EVS-HD 60364 standard), which
    // is exactly the kind of code that's cheap to test and expensive to get wrong silently.
    //
    // These tests rely on the seed data already baked into ElektriKalkulaatorContext (see
    // TestBase.cs) — the same "korterelamu"/"eramu"/"ärihoone" rules and ABB/Schneider/Draka
    // products the real app ships with. That's deliberate: it means these tests double as a
    // safety net for the seed data itself, not just the Calculate() method.
    public class CalculatorServicesTests : TestBase
    {
        // A helper to build a minimal, valid input — each test only overrides the fields it
        // actually cares about, instead of repeating every property in every test.
        private static CalculatorInputDto KorterelamuInput(
            int roomCount = 2,
            int lightCount = 0,
            int socketCount = 0,
            bool hasElectricStove = false)
        {
            return new CalculatorInputDto
            {
                BuildingType = "korterelamu",
                RoomCount = roomCount,
                LightCount = lightCount,
                SocketCount = socketCount,
                HasElectricStove = hasElectricStove
            };
        }

        [Fact]
        public async Task Calculate_With8Lights_AddsExactlyOneLightingCircuit()
        {
            // Arrange: the rule for lighting is "1 circuit per 8 lights", so exactly 8 lights
            // should need exactly 1 circuit (not round up to 2).
            var input = KorterelamuInput(roomCount: 2, lightCount: 8);

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert
            // Note: both the breaker AND the wire line carry the rule's WireCrossSectionMm2 (see
            // CalculatorServices.Calculate() — it copies rule.WireCrossSectionMm2 onto both), so
            // "WireCrossSectionMm2 == null" does NOT distinguish them. Filtering out the cable
            // line by name does.
            var breaker = Assert.Single(bom, item => item.CircuitType == "lighting" && !item.ProductName.Contains("kaabel"));
            // Two 10A breakers exist in the seed data (ABB 8.50€, Schneider 7.90€) — the cheaper
            // one must win, proving the "pick cheapest in-stock match" logic actually works.
            Assert.Equal("Schneider Easy9 B10A", breaker.ProductName);
            Assert.Equal(1, breaker.Quantity);
            Assert.Equal(7.90m, breaker.UnitPrice);
        }

        [Fact]
        public async Task Calculate_With9Lights_RoundsUpToTwoLightingCircuits()
        {
            // Arrange: 9 lights is "1 circuit per 8 lights" rounded UP — this specifically tests
            // the Math.Ceiling() call, not just plain division (9 / 8 = 1 with integer division,
            // which would be the wrong answer here).
            var input = KorterelamuInput(lightCount: 9);

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert
            var breaker = bom.Single(item => item.CircuitType == "lighting" && item.ProductName.Contains("B10"));
            Assert.Equal(2, breaker.Quantity);
            Assert.Equal(15.80m, breaker.TotalPrice); // 2 x 7.90
        }

        [Fact]
        public async Task Calculate_With6Sockets_AddsOneSocketCircuit()
        {
            // Arrange: sockets follow "1 circuit per 6 sockets".
            var input = KorterelamuInput(socketCount: 6);

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert
            var breaker = bom.Single(item => item.CircuitType == "socket" && item.ProductName.Contains("B16"));
            Assert.Equal("Schneider Easy9 B16A", breaker.ProductName); // cheaper than the ABB 16A
            Assert.Equal(1, breaker.Quantity);

            var wire = bom.Single(item => item.CircuitType == "socket" && item.ProductName.Contains("kaabel"));
            Assert.Equal(2.5m, wire.WireCrossSectionMm2);
        }

        [Fact]
        public async Task Calculate_WithElectricStove_AddsExactlyOneStoveCircuit()
        {
            // Arrange: unlike lighting/sockets, the stove circuit isn't derived from a count —
            // it's a fixed "1 dedicated circuit if the checkbox is ticked".
            var input = KorterelamuInput(hasElectricStove: true);

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert
            var breaker = bom.Single(item => item.CircuitType == "stove" && !item.ProductName.Contains("kaabel"));
            Assert.Equal("ABB S201-B32 Kaitselüliti 32A", breaker.ProductName);
            Assert.Equal(1, breaker.Quantity);

            var wire = bom.Single(item => item.CircuitType == "stove" && item.ProductName.Contains("kaabel"));
            Assert.Equal(6.0m, wire.WireCrossSectionMm2);
        }

        [Fact]
        public async Task Calculate_WithoutElectricStove_AddsNoStoveCircuit()
        {
            // Arrange
            var input = KorterelamuInput(hasElectricStove: false);

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert
            Assert.DoesNotContain(bom, item => item.CircuitType == "stove");
        }

        [Fact]
        public async Task Calculate_AlwaysAddsEnclosureAndRcd_EvenWithZeroCircuitsRequested()
        {
            // Arrange: no lights, no sockets, no stove — but the building type is still valid.
            // This pins down real (if slightly surprising) behaviour: as long as the building
            // type matches a known rule set, an enclosure and RCD are always added, regardless
            // of whether any circuits were actually requested.
            var input = KorterelamuInput(lightCount: 0, socketCount: 0, hasElectricStove: false);

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert
            Assert.Equal(2, bom.Count);
            Assert.Contains(bom, item => item.CircuitType == "panel" && item.ProductName.Contains("Mistral"));
            Assert.Contains(bom, item => item.CircuitType == "rcd" && item.ProductName.Contains("RCD"));
        }

        [Fact]
        public async Task Calculate_ForUnknownBuildingType_ReturnsEmptyBom()
        {
            // Arrange: not one of "korterelamu" / "eramu" / "ärihoone" — no rules match at all,
            // so Calculate() should bail out before adding even the enclosure/RCD.
            var input = KorterelamuInput();
            input.BuildingType = "kosmosejaam"; // "space station" — deliberately not a real option

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert
            Assert.Empty(bom);
        }

        [Fact]
        public async Task Calculate_IgnoresOutOfStockProducts_EvenIfCheaper()
        {
            // Arrange: add a fake 10A breaker that's cheaper than every real seeded option, but
            // out of stock. If Calculate() only sorted by price and forgot the stock check, this
            // product would win — this test exists specifically to catch that mistake.
            Context.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = Context.ProductCategories.Single(c => c.Name == "Kaitselülitid").Id,
                Name = "Odav aga otsas olev kaitselüliti",
                Brand = "NoName",
                RatedCurrent = 10,
                Voltage = 230,
                Price = 0.01m, // cheapest possible price
                StockQuantity = 0, // but not actually available
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            });
            await Context.SaveChangesAsync();

            var input = KorterelamuInput(lightCount: 8);

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert: the real, in-stock Schneider breaker should still be chosen.
            var breaker = bom.Single(item => item.CircuitType == "lighting" && !item.ProductName.Contains("kaabel"));
            Assert.Equal("Schneider Easy9 B10A", breaker.ProductName);
        }

        [Fact]
        public async Task Calculate_WireQuantity_ScalesWithRoomCount()
        {
            // Arrange: the wire estimate is "8 metres of cable per room, per circuit".
            var input = KorterelamuInput(roomCount: 5, lightCount: 8); // exactly 1 lighting circuit

            // Act
            var bom = await Svc<ICalculatorServices>().Calculate(input);

            // Assert: 1 circuit x (5 rooms x 8 metres) = 40 metres.
            var wire = bom.Single(item => item.CircuitType == "lighting" && item.ProductName.Contains("kaabel"));
            Assert.Equal(40, wire.Quantity);
            Assert.Equal(48.00m, wire.TotalPrice); // 40m x 1.20€/m
        }

        [Fact]
        public async Task SaveCalculation_PersistsHeaderRequirementsAndAllBomLines()
        {
            // Arrange
            var input = KorterelamuInput(lightCount: 8, socketCount: 6, hasElectricStove: true);
            var calculatorServices = Svc<ICalculatorServices>();
            var bom = await calculatorServices.Calculate(input);

            // Act
            var saved = await calculatorServices.SaveCalculation(input, bom);

            // Assert: one calculation header row, with the total cost matching the BOM's sum...
            Assert.Equal(bom.Sum(item => item.TotalPrice), saved.TotalCost);

            // ...one requirements row linked to it, holding exactly what the user typed in...
            var requirements = Context.PowerboxRequirements.Single(r => r.CalculationId == saved.Id);
            Assert.Equal(input.RoomCount, requirements.RoomCount);
            Assert.Equal(input.BuildingType, requirements.BuildingType);

            // ...and one PowerboxComponents row per BOM line — nothing lost, nothing duplicated.
            var componentCount = Context.PowerboxComponents.Count(c => c.CalculationId == saved.Id);
            Assert.Equal(bom.Count, componentCount);
        }
    }
}
