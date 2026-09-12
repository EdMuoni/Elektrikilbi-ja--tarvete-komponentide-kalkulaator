using System.Net;

namespace ElektriKalkulaator.Tests.Integration
{
    // Protections that apply to requests themselves: antiforgery, redirect safety, input limits.
    //
    // Automated versions of the remaining checks in scripts/security-check.sh. Each one had to be
    // reproduced as a working exploit before it was fixed, and each test here is that exploit
    // turned into something that runs on every build.
    public class RequestSecurityTests : IClassFixture<TestWebAppFactory>
    {
        private const string SeededProductId = "22222222-0000-0000-0000-000000000002";

        private readonly TestWebAppFactory _factory;

        public RequestSecurityTests(TestWebAppFactory factory) => _factory = factory;

        // ── Cross-site request forgery ──────────────────────────────────────────
        //
        // Without a token check, another website can make a visitor's browser submit these forms
        // in the background — emptying their cart, or filling our database with junk calculations.

        public static TheoryData<string, Dictionary<string, string>> UnprotectedPostAttempts() => new()
        {
            { "/Cart/Add",      new() { ["productId"] = SeededProductId, ["quantity"] = "1" } },
            { "/Cart/Remove",   new() { ["productId"] = SeededProductId } },
            { "/Cart/Clear",    new() },
            { "/Cart/Checkout", new() },
            { "/Calculator",    new() { ["BuildingType"] = "eramu", ["RoomCount"] = "3",
                                        ["SocketCount"] = "6", ["LightCount"] = "8" } }
        };

        [Theory]
        [MemberData(nameof(UnprotectedPostAttempts))]
        public async Task PostWithoutAnAntiforgeryToken_IsRefused(string url, Dictionary<string, string> fields)
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.PostAsync(url, new FormUrlEncodedContent(fields));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostWithAValidToken_IsAccepted()
        {
            // The mirror of the test above. Without this one, breaking the forms entirely would
            // still leave every antiforgery test passing.
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.PostFormAsync("/Products", "/Cart/Add",
                new Dictionary<string, string> { ["productId"] = SeededProductId, ["quantity"] = "1" });

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        }

        // ── Open redirect ───────────────────────────────────────────────────────
        //
        // Cart/Add takes a returnUrl and used to hand it straight to Redirect(). A link that
        // genuinely started on this site could therefore land the visitor on an attacker's copy of
        // the login page.

        [Theory]
        [InlineData("https://evil.example.com/phish")]
        [InlineData("http://evil.example.com")]
        [InlineData("//evil.example.com")]              // protocol-relative — still leaves the site
        [InlineData("https://evil.example.com\\@localhost")]
        public async Task ReturnUrlPointingOffSite_IsIgnored(string hostileUrl)
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.PostFormAsync("/Products", "/Cart/Add",
                new Dictionary<string, string>
                {
                    ["productId"] = SeededProductId,
                    ["quantity"] = "1",
                    ["returnUrl"] = hostileUrl
                });

            var destination = response.Headers.Location?.ToString() ?? "";

            Assert.DoesNotContain("evil.example.com", destination);
            Assert.Equal("/Cart", response.RedirectPath());
        }

        [Fact]
        public async Task ReturnUrlPointingAtThisSite_IsHonoured()
        {
            // The guard must not be so blunt that it breaks the feature it protects.
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.PostFormAsync("/Products", "/Cart/Add",
                new Dictionary<string, string>
                {
                    ["productId"] = SeededProductId,
                    ["quantity"] = "1",
                    ["returnUrl"] = "/Products"
                });

            Assert.Equal("/Products", response.RedirectPath());
        }

        // ── Cart input ──────────────────────────────────────────────────────────

        [Theory]
        [InlineData("-5")]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("1000")]     // above the 999 maximum
        [InlineData("999999")]
        public async Task QuantitiesOutsideTheAllowedRange_DoNotEnterTheCart(string quantity)
        {
            var client = _factory.CreateNonRedirectingClient();

            await client.PostFormAsync("/Products", "/Cart/Add",
                new Dictionary<string, string>
                {
                    ["productId"] = SeededProductId,
                    ["quantity"] = quantity
                });

            var cartHtml = await client.GetStringAsync("/Cart");

            // A negative quantity used to produce a negative line total, which would reduce the
            // amount owed once real payment exists.
            Assert.DoesNotContain("-", ExtractTotals(cartHtml));
        }

        [Fact]
        public async Task AValidQuantity_DoesEnterTheCart()
        {
            var client = _factory.CreateNonRedirectingClient();

            await client.PostFormAsync("/Products", "/Cart/Add",
                new Dictionary<string, string> { ["productId"] = SeededProductId, ["quantity"] = "2" });

            var cartHtml = await client.GetStringAsync("/Cart");

            Assert.Contains("2 tk", cartHtml);
        }

        [Fact]
        public async Task AddingAProductThatDoesNotExist_Returns404()
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.PostFormAsync("/Products", "/Cart/Add",
                new Dictionary<string, string>
                {
                    ["productId"] = "99999999-9999-9999-9999-999999999999",
                    ["quantity"] = "1"
                });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RequestingAProductThatDoesNotExist_Returns404_NotAServerError()
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.GetAsync("/Products/Details/99999999-9999-9999-9999-999999999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Pulls out just the money figures so the assertion is not confused by hyphens elsewhere
        // in the page, such as inside a GUID or a product name.
        private static string ExtractTotals(string html)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(html, @"-?\d+[.,]\d{2}\s*€");
            return string.Join(" ", matches.Select(m => m.Value));
        }
    }
}
