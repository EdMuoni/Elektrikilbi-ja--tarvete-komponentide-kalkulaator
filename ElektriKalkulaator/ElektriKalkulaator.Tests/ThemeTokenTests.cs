using System.Text.RegularExpressions;

namespace ElektriKalkulaator.Tests
{
    // Guards the colour system: every colour must come from a theme token, never from a literal
    // value written into a view.
    //
    // Why this exists — a real bug, not a hypothetical one:
    // On 2026-08-14 we found 38 uses of Bootstrap's `text-white` spread across 14 views.
    // `text-white` is defined as literally `color: #fff`. That is fine on a dark background and
    // invisible on a light one, so every heading using it became white text on a white card the
    // moment anyone switched to the light theme. The light theme had shipped three days earlier.
    //
    // The painful part: the colour tokens in theme.css were all correct. The markup was simply
    // going around them. A token system only works if nothing bypasses it, and until now nothing
    // checked that.
    //
    // These tests are plain text searches over .cshtml files — no browser, no rendering, no
    // database. They run in milliseconds and catch the whole family of "hard-coded colour" bugs.
    //
    // What they CANNOT do: tell you whether the palette actually looks good. That still needs a
    // person to look at the screen. See docs/TESTING.md Part 7.
    public class ThemeTokenTests
    {
        // Walks up from the test binary (bin/Debug/net9.0) until it finds the web project.
        // Same approach as SeedDataIntegrityTests.
        private static string ViewsPath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "ElektriKalkulaator", "Views")))
                dir = dir.Parent;

            Assert.NotNull(dir); // if this fails the test project has been moved
            return Path.Combine(dir!.FullName, "ElektriKalkulaator", "Views");
        }

        private static IEnumerable<string> AllViews() =>
            Directory.EnumerateFiles(ViewsPath(), "*.cshtml", SearchOption.AllDirectories);

        // Turns an absolute path into something readable in a failure message.
        private static string Short(string path)
        {
            var i = path.IndexOf("Views", StringComparison.Ordinal);
            return i >= 0 ? path[i..] : path;
        }

        [Fact]
        public void NoView_UsesABootstrapColourClassThatIgnoresTheTheme()
        {
            // Each of these hard-codes a colour inside Bootstrap's own stylesheet, so it stays the
            // same when the theme changes — which is precisely the bug described above. The
            // token-based replacements live in site.css (.text-strong-custom, .text-muted-custom,
            // .text-amber, .bg-card).
            //
            // `navbar-dark` is in the list because it caused this exact bug once already: it forces
            // light link text, giving white-on-white in the light theme.
            string[] banned =
            {
                "text-white", "text-black", "text-dark",
                "bg-dark", "bg-light",
                "navbar-dark", "table-dark", "btn-light"
            };

            var offences = new List<string>();

            foreach (var file in AllViews())
            {
                var content = File.ReadAllText(file);

                foreach (var cls in banned)
                {
                    // \b stops "text-dark" matching inside "text-darker", and the negative
                    // lookahead stops "text-white" matching Bootstrap's opacity variants
                    // (text-white-50), which are a separate question from this one.
                    if (Regex.IsMatch(content, $@"\b{Regex.Escape(cls)}\b(?!-)"))
                        offences.Add($"{Short(file)} uses '{cls}'");
                }
            }

            Assert.True(
                offences.Count == 0,
                "Views must take colours from theme tokens, not Bootstrap colour utilities.\n" +
                "Each of these hard-codes a colour and will be wrong in one of the two themes:\n  " +
                string.Join("\n  ", offences));
        }

        [Fact]
        public void NoView_WritesARawColourValueInAnInlineStyle()
        {
            // A hex or rgb() literal in a style="" attribute has the same problem: it cannot follow
            // the theme. Use var(--token) instead — Views/Products/Delete.cshtml shows the pattern.
            var offences = new List<string>();

            foreach (var file in AllViews())
            {
                var lines = File.ReadAllLines(file);

                for (var i = 0; i < lines.Length; i++)
                {
                    // Only look inside style="..." — a hex string elsewhere (an id, a comment,
                    // an SVG path) is not a colour and not our business.
                    foreach (Match style in Regex.Matches(lines[i], @"style\s*=\s*""([^""]*)"""))
                    {
                        var value = style.Groups[1].Value;

                        if (Regex.IsMatch(value, @"#[0-9a-fA-F]{3,8}\b|\brgba?\s*\("))
                            offences.Add($"{Short(file)}:{i + 1}  {value.Trim()}");
                    }
                }
            }

            Assert.True(
                offences.Count == 0,
                "Inline styles must use var(--token), not a literal colour:\n  " +
                string.Join("\n  ", offences));
        }

        [Fact]
        public void BothThemes_DefineExactlyTheSameTokenNames()
        {
            // If a variable exists in the dark palette but not the light one, the light theme
            // silently inherits the DARK value for it — usually an unreadable result that only
            // shows up on one page, in one theme, long after the change that caused it.
            //
            // Comparing the name sets makes that structurally impossible to miss.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ElektriKalkulaator", "wwwroot", "css", "theme.css")))
                dir = dir.Parent;

            Assert.NotNull(dir);
            var css = File.ReadAllText(Path.Combine(dir!.FullName, "ElektriKalkulaator", "wwwroot", "css", "theme.css"));

            // Grab each palette block by its opening selector and read the variable names inside.
            static HashSet<string> TokensIn(string css, string pattern)
            {
                var block = Regex.Match(css, pattern, RegexOptions.Singleline);
                Assert.True(block.Success, $"theme.css no longer contains a block matching: {pattern}");

                return Regex.Matches(block.Groups[1].Value, @"(--[\w-]+)\s*:")
                            .Select(m => m.Groups[1].Value)
                            .ToHashSet();
            }

            var dark  = TokensIn(css, @"\n:root \{(.*?)\n\}");
            var light = TokensIn(css, @":root\[data-theme=""light""\] \{(.*?)\n\}");

            Assert.True(dark.Count > 20, $"Expected a full palette, found only {dark.Count} dark tokens.");

            var missingFromLight = dark.Except(light).OrderBy(x => x).ToList();
            var missingFromDark  = light.Except(dark).OrderBy(x => x).ToList();

            Assert.True(
                missingFromLight.Count == 0 && missingFromDark.Count == 0,
                "The two palettes must define the same token names.\n" +
                $"  missing from light: {string.Join(", ", missingFromLight)}\n" +
                $"  missing from dark:  {string.Join(", ", missingFromDark)}");
        }
    }
}
