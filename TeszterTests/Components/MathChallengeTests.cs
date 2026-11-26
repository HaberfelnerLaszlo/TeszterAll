using System.Net;

using Bunit;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Extensions;
using TeszterLibrary.Services;

using TeszterTests.Helpers;

using TeszterWeb.Components.Pages.Student;

namespace TeszterTests.Components
{
	/// <summary>Időre menő kihívás: Enterre a beírt válasz megy el, és jön a következő feladat.</summary>
	public class MathChallengeTests : BunitContext
	{
		private readonly StubHttpHandler _http = new();

		public MathChallengeTests()
		{
			JSInterop.Mode = JSRuntimeMode.Loose;
			Services.AddFluentUIComponents();
			Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
			{
				["ApiUrl"] = "https://api.test",
				["TeszterApi:BaseUrl"] = "api://abc",
				["TeszterApi:Scopes:0"] = "access_as_user",
			}).Build());
			Services.AddTeszterApiClients();
			Services.AddHttpClient(TeszterApiService.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => _http);
			Services.AddScoped(_ => FakeAuthorizationHeaderProvider.Create("tok"));
			Services.AddScoped<MathService>();
			Services.AddScoped<ClientSignalsTracker>();
			Services.AddSingleton(ToastRecorder.Create(out _));

			_http.Respond = request => request.RequestUri!.AbsolutePath switch
			{
				"/Math/Challenge" => StubHttpHandler.Json(HttpStatusCode.OK, """[{"taskId":3,"name":"Összeadás","best":0}]"""),
				"/Math/Challenge/Start/3" => StubHttpHandler.Json(HttpStatusCode.OK, """
					{"runId":11,"seconds":60,"startsAt":"2026-11-23T10:00:00Z","endsAt":"2026-11-23T10:01:00Z","operations":[
					 {"operationId":1,"sequence":"3 + 4 Ł = Ł"},{"operationId":2,"sequence":"5 + 1 Ł = Ł"}]}
					"""),
				"/Math/Challenge/Answer" => StubHttpHandler.Json(HttpStatusCode.OK, """{"isCorrect":true,"correct":1,"wrong":0,"secondsLeft":55}"""),
				_ => StubHttpHandler.Json(HttpStatusCode.NotFound, null),
			};
		}

		[Fact]
		public async Task Enter_sends_the_typed_answer_and_shows_the_next_task()
		{
			var cut = Render<MathChallenge>();
			await cut.WaitForAssertionAsync(() => Assert.Contains("Összeadás", cut.Markup), TimeSpan.FromSeconds(5));
			cut.FindAll("fluent-button").First(b => b.TextContent.Contains("Összeadás")).Click();
			await cut.WaitForAssertionAsync(() => Assert.Contains("3 + 4", cut.Markup), TimeSpan.FromSeconds(5));

			var input = cut.Find("input.valasz");
			input.Input("7");
			input.KeyDown(new KeyboardEventArgs { Code = "Enter", Key = "Enter" });

			await cut.WaitForAssertionAsync(() => Assert.Contains(_http.Requests, r => r.Uri.EndsWith("/Math/Challenge/Answer") && r.Body.Contains("\"result\":\"7\"")), TimeSpan.FromSeconds(5));
			await cut.WaitForAssertionAsync(() => Assert.Contains("5 + 1", cut.Markup), TimeSpan.FromSeconds(5));
			Assert.DoesNotContain("Ne hagyd üresen", cut.Markup);
		}
	}
}
