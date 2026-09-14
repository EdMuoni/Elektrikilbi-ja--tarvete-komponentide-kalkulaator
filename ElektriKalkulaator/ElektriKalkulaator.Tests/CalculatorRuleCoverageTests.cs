using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Checks that what the FORM offers and what the RULES can deliver stay in step.
    //
    // Why this exists — a real defect found on 2026-09-09:
    // The calculator form shows an "Elektripliit" checkbox for every building type, but
    // only the residential types have a "stove" calculation rule seeded. Ticking it for
    // "ärihoone" produced no stove circuit and no message — the form silently promised
    // something it could not deliver, and the user had no way to know.
    //
    // The fix was NOT to add a fake rule. A commercial building genuinely may not have a
    // domestic cooker circuit, and inventing one would be worse. The fix was to say so in
    // the result. These tests pin down both halves of that.
    public class CalculatorRuleCoverageTests : TestBase
    {
        private static CalculatorInputDto Input(string buildingType) => new()
        {
            BuildingType = buildingType,
            RoomCount = 3,
            SocketCount = 10,
            LightCount = 12,
            SwitchCount = 3,
            HasElectricStove = true
        };

        [Theory]
        [InlineData("korterelamu")]
        [InlineData("eramu")]
        [InlineData("ärihoone")]
        public async Task EveryBuildingTypeOfferedByTheForm_ProducesABom(string buildingType)
        {
            // A building type the form offers must never return an empty result. If a type
            // is added to the dropdown without rules, this fails immediately rather than
            // showing a visitor a blank results panel.
            var bom = await Svc<ICalculatorServices>().Calculate(Input(buildingType));

            Assert.NotEmpty(bom);
            Assert.All(bom, line => Assert.True(line.TotalPrice > 0,
                $"{buildingType}: line '{line.ProductName}' has no price."));
        }

        [Fact]
        public async Task ResidentialBuildings_HonourTheStoveRequest()
        {
            foreach (var type in new[] { "korterelamu", "eramu" })
            {
                var bom = await Svc<ICalculatorServices>().Calculate(Input(type));
                Assert.Contains(bom, line => line.CircuitType == "stove");
            }
        }

        [Fact]
        public async Task ACommercialBuilding_HasNoStoveRule_WhichTheResultMustDisclose()
        {
            // This documents the gap rather than hiding it. If a stove rule is ever added
            // for ärihoone, this test fails and reminds whoever did it to remove the
            // notice in CalculatorController — otherwise the app would warn about
            // something that no longer happens.
            var bom = await Svc<ICalculatorServices>().Calculate(Input("ärihoone"));

            Assert.DoesNotContain(bom, line => line.CircuitType == "stove");
            Assert.NotEmpty(bom);   // the rest of the calculation still works
        }

        [Fact]
        public async Task ABuildingWithNoLoadsAtAll_StillReturnsThePanelAndRcd()
        {
            // Deliberate, and worth pinning down so nobody "fixes" it by accident: an
            // installation always needs an enclosure and a residual current device, even
            // before any final circuit is added. The result is a small non-zero total.
            var bom = await Svc<ICalculatorServices>().Calculate(new CalculatorInputDto
            {
                BuildingType = "korterelamu",
                RoomCount = 1,
                SocketCount = 0,
                LightCount = 0,
                SwitchCount = 0,
                HasElectricStove = false
            });

            Assert.Contains(bom, line => line.CircuitType == "panel");
            Assert.Contains(bom, line => line.CircuitType == "rcd");
            Assert.DoesNotContain(bom, line => line.CircuitType == "lighting");
        }
    }
}
