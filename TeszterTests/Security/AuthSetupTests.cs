using System.Net;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

using TeszterWeb.Services;

namespace TeszterTests.Security
{
    /// <summary>
    /// A Program.cs bejelentkezési beállításai. Régen: az initialScopes a TeszterApi:Scopes tömb miatt mindig a
    /// Graph-ra esett vissza (rejtetten), és két UseExceptionHandler volt, amelyek közül a belső (/Error) elnyelte
    /// az MSAL-ágat, így lejárt token után a felhasználó újrabejelentkezés helyett hibaoldalt kapott.
    /// </summary>
    public class AuthSetupTests
    {
        private static IConfiguration Config(params (string Key, string Value)[] values)
            => new ConfigurationBuilder().AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value))).Build();

        [Fact]
        public void Initial_scopes_are_the_graph_scopes_split_on_any_whitespace()
        {
            var config = Config(
                ("MicrosoftGraph:Scopes", " user.read  eduroster.read\tfiles.readwrite.all "),
                ("TeszterApi:Scopes:0", "access_as_user"));

            Assert.Equal(["user.read", "eduroster.read", "files.readwrite.all"], AuthSetup.InitialScopes(config));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Without_graph_scopes_no_initial_scopes_are_requested(string? scopes)
        {
            var config = scopes is null ? Config() : Config(("MicrosoftGraph:Scopes", scopes));

            Assert.Null(AuthSetup.InitialScopes(config));
        }

        [Fact]
        public void An_msal_interaction_request_is_recognised_at_any_depth()
        {
            var msal = new MsalUiRequiredException("invalid_grant", "lejárt");

            Assert.True(AuthSetup.RequiresInteraction(msal));
            Assert.True(AuthSetup.RequiresInteraction(new HttpRequestException("x", msal)));
            Assert.True(AuthSetup.RequiresInteraction(new InvalidOperationException("a", new AggregateException("b", new Exception("c", msal)))));
            Assert.False(AuthSetup.RequiresInteraction(new InvalidOperationException("más hiba")));
            Assert.False(AuthSetup.RequiresInteraction(null));
        }

        // Ugyanaz a hibakezelési lánc, mint a Program.cs-ben (nem fejlesztői környezetben).
        private static async Task<HttpClient> StartAppAsync()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
            builder.WebHost.UseTestServer();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            builder.Services.AddExceptionHandler<MsalUiRequiredExceptionHandler>();

            var app = builder.Build();
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.MapGet("/Error", () => "hibaoldal");
            app.MapGet("/teacher/results", string () => throw new HttpRequestException("API", new MsalUiRequiredException("invalid_grant", "lejárt")));
            app.MapGet("/broken", string () => throw new InvalidOperationException("valami más"));
            await app.StartAsync();
            return app.GetTestClient();
        }

        [Fact]
        public async Task An_expired_token_signs_out_and_sends_the_user_back_to_the_same_page()
        {
            using var client = await StartAppAsync();

            using var response = await client.GetAsync("/teacher/results?class=8a");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/teacher/results?class=8a", response.Headers.Location?.OriginalString);
            var cookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? string.Join("\n", values) : string.Empty;
            Assert.Contains(".AspNetCore.Cookies=;", cookies);         // a süti törölve: új bejelentkezés indul
            Assert.Contains("expires=Thu, 01 Jan 1970", cookies);
        }

        [Fact]
        public async Task Other_errors_still_show_the_error_page()
        {
            using var client = await StartAppAsync();

            using var response = await client.GetAsync("/broken");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("hibaoldal", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public void The_wasm_leftover_client_config_is_no_longer_served()
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root is not null && !File.Exists(Path.Combine(root.FullName, "TeszterAll.sln")))
            {
                root = root.Parent;
            }
            Assert.NotNull(root);

            // A wwwroot tartalma bejelentkezés nélkül letölthető; konfiguráció nem lehet benne.
            var served = Directory.EnumerateFiles(Path.Combine(root!.FullName, "TeszterWeb", "wwwroot"), "appsettings*.json", SearchOption.AllDirectories);
            Assert.Empty(served);
        }
    }
}
