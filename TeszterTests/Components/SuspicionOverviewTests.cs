using System.Net;
using System.Reflection;

using Bunit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Extensions;
using TeszterLibrary.Services;

using TeszterTests.Helpers;

using TeszterWeb.Components.Pages.Teacher;

namespace TeszterTests.Components
{
	/// <summary>A tanári „Jelzések” oldal: csoportlista, részletek érthetően, tanári döntések; nem vádló nyelvezet.</summary>
	public class SuspicionOverviewTests : BunitContext
	{
		private readonly StubHttpHandler _http = new();
		private static readonly Guid Anna = Guid.Parse("a5000000-0000-0000-0000-0000000000a5");

		private const string StudentJson = """
			{"studentId":"a5000000-0000-0000-0000-0000000000a5","name":"Anna","level":1,"score":12,"serverScore":10,"clientScore":2,
			 "yellowThreshold":10,"redThreshold":25,"windowDays":7,"retentionDays":90,
			 "signals":[{"id":5,"type":5,"title":"Sok válasz rövid idő alatt","description":"25 válasz 60 másodperc alatt.","weight":10,
			             "isClient":false,"createdAt":"2026-11-23T09:00:00Z","dismissed":false,"counted":true}],
			 "badges":[{"code":"villam","name":"Villám","status":2,"statusText":"Megerősítésre vár","firstMetAt":"2026-11-23T09:00:00Z","updatedAt":"2026-11-23T09:00:00Z","decidedByTeacher":false}]}
			""";

		public SuspicionOverviewTests()
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
			Services.AddSingleton(ToastRecorder.Create(out _));

			_http.Respond = request => request.RequestUri!.AbsolutePath switch
			{
				"/Math/Leaderboard" => StubHttpHandler.Json(HttpStatusCode.OK, """[{"id":"7a","name":"7.a"}]"""),
				"/Math/Suspicion/Class/7a" => StubHttpHandler.Json(HttpStatusCode.OK, """
					{"classId":"7a","className":"7.a","windowDays":7,"students":[
					 {"studentId":"a5000000-0000-0000-0000-0000000000a5","name":"Anna","level":1,"score":12,"signalCount":1,"pendingBadges":1,"mainReasons":["Sok válasz rövid idő alatt"]},
					 {"studentId":"b5000000-0000-0000-0000-0000000000b5","name":"Béla","level":0,"score":0,"signalCount":0,"pendingBadges":0,"mainReasons":[]}]}
					"""),
				var p when p.StartsWith("/Math/Suspicion/") => StubHttpHandler.Json(HttpStatusCode.OK, StudentJson),
				_ => StubHttpHandler.Json(HttpStatusCode.NotFound, null),
			};
		}

		private static bool Busy(IRenderedComponent<SuspicionOverview> cut) =>
			(bool)typeof(SuspicionOverview).GetField("busy", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(cut.Instance)!;

		private static Task Call(IRenderedComponent<SuspicionOverview> cut, string method, params object?[] args) =>
			cut.InvokeAsync(() => (Task)typeof(SuspicionOverview).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(cut.Instance, args)!);

		[Fact]
		public async Task Class_students_details_and_teacher_actions()
		{
			var cut = Render<SuspicionOverview>();
			await Call(cut, "SelectClass", "7a");
			cut.Render();   // a közvetlenül hívott metódus után (nem eseménykezelő) újrarajzolás
			Assert.Contains("Béla", cut.Markup);
			Assert.Equal(2, cut.FindAll("fluent-button").Count(b => b.TextContent.Contains("Részletek")));
			Assert.Contains("Érdemes ránézni", cut.Markup);
			Assert.Contains("nem bizonyítékok", cut.Markup);

			await Call(cut, "SelectStudent", Anna);
			cut.Render();
			Assert.Contains("25 válasz 60 másodperc alatt.", cut.Markup);
			Assert.Contains("Megerősítésre vár", cut.Markup);

			cut.FindAll("fluent-button").First(b => b.TextContent.Contains("Rendben, nem gond")).Click();
			await cut.WaitForAssertionAsync(() => Assert.Contains(_http.Requests, r => r.Method == "POST" && r.Uri.EndsWith("/Math/Suspicion/Signal/5/Dismiss")), TimeSpan.FromSeconds(5));
			// A művelet a csoportlistát is frissíti; amíg tart (busy), az oldal szándékosan nem fogad újabb műveletet.
			await cut.WaitForStateAsync(() => !Busy(cut), TimeSpan.FromSeconds(5));

			cut.FindAll("fluent-button").First(b => b.TextContent.Contains("Megadom")).Click();
			await cut.WaitForAssertionAsync(() => Assert.Contains(_http.Requests, r => r.Method == "POST" && r.Uri.EndsWith($"/Math/Suspicion/Student/{Anna}/Badge/villam/Confirm")), TimeSpan.FromSeconds(5));

			Assert.DoesNotContain("csal", cut.Markup, StringComparison.OrdinalIgnoreCase);
		}
	}
}
