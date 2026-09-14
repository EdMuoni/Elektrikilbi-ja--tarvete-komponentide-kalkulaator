using System.Net;
using System.Text.RegularExpressions;

namespace ElektriKalkulaator.Tests.Integration
{
    // Small helpers shared by the integration tests.
    //
    // Two things every test needs and neither is obvious the first time:
    //   1. ASP.NET refuses any POST without a matching antiforgery token, so a test that wants to
    //      exercise a form must first fetch the page and read the hidden token out of the HTML.
    //   2. Logging in means posting to /Account/Login and keeping the cookie that comes back —
    //      HttpClient does that automatically as long as the same client instance is reused.
    public static class HttpTestHelpers
    {
        // Finds the hidden antiforgery field that Razor puts inside every form.
        private static readonly Regex TokenPattern = new(
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.Compiled);

        // Fetches a page and extracts its antiforgery token.
        // The cookie that pairs with the token is stored by the HttpClient automatically.
        public static async Task<string> GetAntiforgeryTokenAsync(this HttpClient client, string url)
        {
            var html = await client.GetStringAsync(url);
            var match = TokenPattern.Match(html);

            Assert.True(match.Success, $"No antiforgery token found on {url}. Does that page contain a form?");
            return match.Groups[1].Value;
        }

        // Posts form values along with a freshly fetched antiforgery token.
        // tokenSourceUrl is the page whose form we are pretending to submit.
        public static async Task<HttpResponseMessage> PostFormAsync(
            this HttpClient client,
            string tokenSourceUrl,
            string postUrl,
            Dictionary<string, string> fields)
        {
            var token = await client.GetAntiforgeryTokenAsync(tokenSourceUrl);

            var payload = new Dictionary<string, string>(fields)
            {
                ["__RequestVerificationToken"] = token
            };

            return await client.PostAsync(postUrl, new FormUrlEncodedContent(payload));
        }

        // Signs in and returns whether it worked.
        // A successful login answers with a redirect; a failed one re-renders the form with 200,
        // which is why the status code alone tells the two apart.
        public static async Task<bool> LoginAsync(this HttpClient client, string email, string password)
        {
            var response = await client.PostFormAsync("/Account/Login", "/Account/Login",
                new Dictionary<string, string>
                {
                    ["Email"] = email,
                    ["Password"] = password,
                    ["RememberMe"] = "false"
                });

            return response.StatusCode == HttpStatusCode.Redirect
                || response.StatusCode == HttpStatusCode.Found;
        }

        // The path part of a redirect's Location header, e.g. "/Account/Login".
        // Returns "" when the response was not a redirect, so assertions read cleanly.
        public static string RedirectPath(this HttpResponseMessage response)
        {
            var location = response.Headers.Location;
            if (location == null) return "";

            return location.IsAbsoluteUri ? location.AbsolutePath : location.OriginalString.Split('?')[0];
        }
    }
}
