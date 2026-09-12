namespace ElektriKalkulaator.Tests.Integration
{
    // What happens to session data when someone signs out.
    //
    // Why this matters: the shopping cart lives in the session, not against the user account. On a
    // shared computer — a library, a workshop, a college machine — the next person to use the
    // browser would inherit whatever the previous person had put in their cart. Signing out is
    // exactly the moment a user expects their traces to be removed.
    public class SessionPrivacyTests : IClassFixture<TestWebAppFactory>
    {
        private const string SeededProductId = "22222222-0000-0000-0000-000000000002";

        private readonly TestWebAppFactory _factory;

        public SessionPrivacyTests(TestWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task SigningOut_EmptiesTheCart()
        {
            await _factory.EnsureCustomerExistsAsync();
            var client = _factory.CreateNonRedirectingClient();

            await client.LoginAsync(TestWebAppFactory.CustomerEmail, TestWebAppFactory.CustomerPassword);

            // Put something in the cart while signed in.
            await client.PostFormAsync("/Products", "/Cart/Add",
                new Dictionary<string, string> { ["productId"] = SeededProductId, ["quantity"] = "3" });

            Assert.Contains("3 tk", await client.GetStringAsync("/Cart"));

            // Sign out — the same browser is now effectively a different person.
            await client.PostFormAsync("/", "/Account/Logout", new Dictionary<string, string>());

            var cartAfterLogout = await client.GetStringAsync("/Cart");

            Assert.DoesNotContain("3 tk", cartAfterLogout);
            // Assert on the marker, not on the wording. This test is about the session
            // being cleared; it should not fail because someone improved a sentence.
            Assert.Contains("data-cart-state=\"empty\"", cartAfterLogout);
        }

        [Fact]
        public async Task AnAnonymousVisitorsCart_StillWorksNormally()
        {
            // Signing out must not be so aggressive that it breaks carts for people who never
            // signed in — the cart is deliberately usable without an account.
            var client = _factory.CreateNonRedirectingClient();

            await client.PostFormAsync("/Products", "/Cart/Add",
                new Dictionary<string, string> { ["productId"] = SeededProductId, ["quantity"] = "2" });

            Assert.Contains("2 tk", await client.GetStringAsync("/Cart"));
        }
    }
}
