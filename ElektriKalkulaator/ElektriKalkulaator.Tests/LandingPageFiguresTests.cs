using System.Text.RegularExpressions;
using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;

namespace ElektriKalkulaator.Tests
{
    // Keeps the numbers printed on the landing page honest.
    //
    // Why this exists — a real bug, found on 2026-08-14:
    // The home page advertised a worked example of "160 m paigalduskaablit" costing
    // "504,10 €". Running the calculator with the inputs that example describes actually
    // returns 120 m of cable and 348.90 €. The figures had been hand-written into the view
    // and had drifted from the code, and nothing anywhere noticed.
    //
    // For most sites that is a typo. For this one it is the worst possible bug: the entire
    // claim of the project is that its quantities are derived from EVS-HD 60364 and can be
    // audited line by line. A made-up number on the front page undermines exactly that.
    //
    // So the example is now computed, not asserted from memory: this test runs the real
    // calculator and checks the view agrees with it.
    public class LandingPageFiguresTests : TestBase
    {
        // These are the inputs the worked example describes, and they are repeated in a
        // comment in Views/Home/Index.cshtml. If you change one, change both.
        private static CalculatorInputDto WorkedExampleInput() => new()
        {
            BuildingType = "korterelamu",
            RoomCount = 3,
            SocketCount = 10,
            LightCount = 12,
            HasElectricStove = true
        };

        private static string HomePagePath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ElektriKalkulaator", "Views", "Home", "Index.cshtml")))
                dir = dir.Parent;

            Assert.NotNull(dir); // if this fails the test project has been moved
            return Path.Combine(dir!.FullName, "ElektriKalkulaator", "Views", "Home", "Index.cshtml");
        }

        // Returns the page with its Razor comments removed.
        //
        // This is not a detail - it is the difference between a working test and a
        // decorative one. The comment above the worked example documents the figures in
        // prose ("of which 120 m is cable"), so a test searching the raw file finds the
        // right number inside the comment even when the number a visitor actually SEES is
        // wrong. Verified by mutation: with the comment left in, changing the visible
        // "120 m" to "1600 m" still passed.
        private static string VisibleMarkup()
        {
            var html = File.ReadAllText(HomePagePath());
            return Regex.Replace(html, @"@\*.*?\*@", "", RegexOptions.Singleline);
        }

        [Fact]
        public async Task TheWorkedExampleTotal_MatchesWhatTheCalculatorActuallyReturns()
        {
            var bom = await Svc<ICalculatorServices>().Calculate(WorkedExampleInput());
            var total = bom.Sum(line => line.TotalPrice);

            // The view writes the total the Estonian way, with a comma: "348,90 €".
            var expected = total.ToString("F2").Replace('.', ',');
            var html = VisibleMarkup();

            Assert.True(
                html.Contains(expected + " €", StringComparison.Ordinal),
                $"The landing page must show the total the calculator actually produces.\n" +
                $"  calculator returns : {expected} €\n" +
                $"  not found in       : Views/Home/Index.cshtml\n" +
                $"Re-run the worked example and update BOTH the list and the hero proof card.");
        }

        [Fact]
        public async Task EveryCableLengthOnThePage_MatchesWhatTheCalculatorActuallyReturns()
        {
            var bom = await Svc<ICalculatorServices>().Calculate(WorkedExampleInput());

            // Cable is the line measured in metres; everything else is counted in pieces.
            // This is the family of figures that was wrong once already: the page advertised
            // 160 m where the calculator produced 120 m.
            //
            // The hero used to carry a combined "120 m", but that card was removed, so the
            // remaining figures are the three per-circuit lengths in the worked example.
            // Checking each one is a stronger guarantee than checking their sum: a total can
            // still be right while the individual lines are wrong.
            var cableLengths = bom.Where(line => line.Unit == "m")
                                  .Select(line => line.Quantity)
                                  .ToList();

            Assert.NotEmpty(cableLengths);

            var html = VisibleMarkup();
            var missing = cableLengths
                .Where(m => !html.Contains($"{m} m", StringComparison.Ordinal))
                .ToList();

            Assert.True(
                missing.Count == 0,
                "The worked example must show every cable length the calculator produces." + "\n" +
                $"  calculator returns : {string.Join(", ", cableLengths.Select(m => m + " m"))}" + "\n" +
                $"  not found on page  : {string.Join(", ", missing.Select(m => m + " m"))}");
        }

        [Fact]
        public async Task TheStatStrip_ClaimsTheRealNumberOfBomLines()
        {
            var bom = await Svc<ICalculatorServices>().Calculate(WorkedExampleInput());
            var html = VisibleMarkup();

            // Pull the number out of the stat whose label mentions the worked example, rather
            // than searching the whole page for the digit - the digit alone appears all over.
            var match = Regex.Match(
                html,
                @"<span class=""stat-value"">(\d+)</span>\s*<span class=""stat-label"">rida näidisarvutuses</span>",
                RegexOptions.Singleline);

            Assert.True(match.Success, "The stat strip no longer has a 'rida näidisarvutuses' entry.");
            Assert.Equal(bom.Count, int.Parse(match.Groups[1].Value));
        }
    }
}
