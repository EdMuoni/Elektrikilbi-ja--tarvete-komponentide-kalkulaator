using System.Net;

namespace ElektriKalkulaator.Tests.Integration
{
    // Who may open which page.
    //
    // This is the automated version of the checks in scripts/security-check.sh. It exists because
    // the most serious problem ever found in this project was that EVERY admin page was reachable
    // by anyone who knew the URL — Program.cs called UseAuthorization() without UseAuthentication(),
    // so the authorization middleware had nobody to check against and silently allowed everything.
    //
    // Nothing outside a running application can catch that. These tests can.
    public class AuthorizationTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public AuthorizationTests(TestWebAppFactory factory) => _factory = factory;

        // Pages only an administrator may open.
        public static TheoryData<string> AdminOnlyPages() => new()
        {
            "/Products/Create",
            "/Products/Edit/22222222-0000-0000-0000-000000000002",
            "/Products/Delete/22222222-0000-0000-0000-000000000002",
            "/Products/Categories"
        };

        // Pages anyone may open, signed in or not. The calculator and catalogue are the product;
        // putting them behind a login would defeat the point of the site.
        public static TheoryData<string> PublicPages() => new()
        {
            "/",
            "/Products",
            "/Calculator",
            "/Cart",
            "/Products/Details/22222222-0000-0000-0000-000000000002"
        };

        [Theory]
        [MemberData(nameof(AdminOnlyPages))]
        public async Task AnonymousVisitor_IsSentToTheLoginPage(string url)
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/Account/Login", response.RedirectPath());
        }

        [Theory]
        [MemberData(nameof(PublicPages))]
        public async Task AnonymousVisitor_CanOpenPublicPages(string url)
        {
            var client = _factory.CreateNonRedirectingClient();

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(AdminOnlyPages))]
        public async Task AdministratorCanOpenAdminPages(string url)
        {
            var client = _factory.CreateNonRedirectingClient();
            Assert.True(await client.LoginAsync(TestWebAppFactory.AdminEmail, TestWebAppFactory.AdminPassword),
                "The seeded administrator could not sign in.");

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // The most important distinction in the whole authorization setup.
        //
        // A signed-in customer is AUTHENTICATED but not AUTHORIZED for admin pages. They must be
        // shown "access denied", not the login page — sending them to log in again when they
        // already are is a confusing dead end, and it also leaks that the page exists.
        [Theory]
        [MemberData(nameof(AdminOnlyPages))]
        public async Task SignedInCustomer_IsRefusedWithAccessDenied_NotAskedToLogInAgain(string url)
        {
            await _factory.EnsureCustomerExistsAsync();

            var client = _factory.CreateNonRedirectingClient();
            Assert.True(await client.LoginAsync(TestWebAppFactory.CustomerEmail, TestWebAppFactory.CustomerPassword),
                "The test customer could not sign in.");

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("/Account/AccessDenied", response.RedirectPath());
        }

        [Fact]
        public async Task SignedInCustomer_CanStillUseTheCalculatorAndCatalogue()
        {
            await _factory.EnsureCustomerExistsAsync();

            var client = _factory.CreateNonRedirectingClient();
            await client.LoginAsync(TestWebAppFactory.CustomerEmail, TestWebAppFactory.CustomerPassword);

            foreach (var url in new[] { "/Calculator", "/Products", "/Cart" })
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(url)).StatusCode);
        }

        [Fact]
        public async Task WrongPassword_DoesNotSignAnyoneIn()
        {
            var client = _factory.CreateNonRedirectingClient();

            var loggedIn = await client.LoginAsync(TestWebAppFactory.AdminEmail, "TotallyWrongPassword1");

            Assert.False(loggedIn);
            // And the admin pages stay closed.
            var response = await client.GetAsync("/Products/Create");
            Assert.Equal("/Account/Login", response.RedirectPath());
        }

        [Fact]
        public async Task UnknownEmail_DoesNotSignAnyoneIn()
        {
            var client = _factory.CreateNonRedirectingClient();

            Assert.False(await client.LoginAsync("nobody@nowhere.test", "AnyPassword123"));
        }

        [Fact]
        public async Task LoggingOut_RemovesAccessAgain()
        {
            var client = _factory.CreateNonRedirectingClient();
            await client.LoginAsync(TestWebAppFactory.AdminEmail, TestWebAppFactory.AdminPassword);

            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Products/Create")).StatusCode);

            await client.PostFormAsync("/", "/Account/Logout", new Dictionary<string, string>());

            var afterLogout = await client.GetAsync("/Products/Create");
            Assert.Equal("/Account/Login", afterLogout.RedirectPath());
        }
    }
}
