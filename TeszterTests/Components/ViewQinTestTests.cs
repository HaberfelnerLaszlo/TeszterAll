using System.Diagnostics;
using System.Reflection;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Components.Teszt;
using TeszterLibrary.Extensions;
using TeszterLibrary.Services;
using TeszterLibrary.ViewModel;
using TeszterTests.Helpers;

namespace TeszterTests.Components
{
    /// <summary>
    /// A teszt-kitöltő komponens időkorlátja, beküldési zárja és életciklusa. Valós időt mér, ezért néhány teszt több másodperces.
    /// Az eredeti (async void-os) változaton ezek mind elbuktak: az időzítő el sem indult, három egyidejű "Következő"
    /// három kérdést vitt el, a rögzített idő 0 volt, és a szülő újrarenderelése elölről indította a tesztet.
    /// </summary>
    public class ViewQinTestTests : BunitContext
    {
        private readonly TestRunApiHandler _api = new();

        public ViewQinTestTests()
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
            Services.AddHttpClient(TeszterApiService.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => _api);
            Services.AddScoped(_ => FakeAuthorizationHeaderProvider.Create("tok"));
            Services.AddScoped<TestUserServices>();
        }

        private IRenderedComponent<ViewQinTest> Render(bool timer, Action<List<QuestionView>>? onEnd = null, int id = 7)
            => Render<ViewQinTest>(p => p
                .Add(c => c.Id, id)
                .Add(c => c.IsTimer, timer)
                .Add(c => c.TestEnd, onEnd ?? (_ => { })));

        // A komponens privát tagjait reflexióval érjük el: a viselkedést teszteljük, nem a megjelenítést.
        private static Task Call(IRenderedComponent<ViewQinTest> cut, string method)
            => cut.InvokeAsync(() => (Task)typeof(ViewQinTest)
                .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(cut.Instance, new object[] { new List<AnswerView>() })!);

        private static int RemainingQuestions(IRenderedComponent<ViewQinTest> cut)
            => ((List<QuestionView>)typeof(ViewQinTest).GetField("questions", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(cut.Instance)!).Count;

        [Fact]
        public async Task The_time_limit_advances_the_whole_test_by_itself()
        {
            var ended = new List<List<QuestionView>>();
            var stopwatch = Stopwatch.StartNew();
            var cut = Render(timer: true, onEnd: ended.Add);

            await cut.WaitForStateAsync(() => ended.Count > 0, TimeSpan.FromSeconds(15));

            Assert.Single(ended);
            Assert.Equal(3, ended[0].Count);
            Assert.Equal(3, _api.Results);
            Assert.All(_api.ReportedTimes, t => Assert.Equal(2, t));          // a beküldött idő = az időkorlát
            Assert.InRange(stopwatch.Elapsed.TotalSeconds, 5.5, 9.5);          // 3 x 2 mp + hálózati késleltetés; nem csúszik +1 mp-ekkel
        }

        [Fact]
        public async Task Simultaneous_next_clicks_submit_only_once()
        {
            _api.LatencyMs = 150;
            var cut = Render(timer: false);
            await cut.WaitForStateAsync(() => _api.Gets == 1 && RemainingQuestions(cut) == 3, TimeSpan.FromSeconds(5));

            await Task.WhenAll(Call(cut, "Next"), Call(cut, "Next"), Call(cut, "Next"));

            Assert.Equal(1, _api.Results);
            Assert.Equal(2, RemainingQuestions(cut));
        }

        [Fact]
        public async Task A_manual_next_reports_the_real_elapsed_seconds()
        {
            _api.QuestionTime = 30;
            var cut = Render(timer: true);
            await cut.WaitForStateAsync(() => _api.Gets == 1, TimeSpan.FromSeconds(5));
            await Task.Delay(1300);

            await Call(cut, "Next");

            Assert.Equal(1, Assert.Single(_api.ReportedTimes));
        }

        [Fact]
        public async Task A_parent_rerender_does_not_restart_the_test_but_a_new_id_reloads_it()
        {
            _api.QuestionTime = 30;
            var cut = Render(timer: false);
            await cut.WaitForStateAsync(() => _api.Gets == 1, TimeSpan.FromSeconds(5));
            await Call(cut, "Next");

            cut.Render(p => p.Add(c => c.Id, 7));
            cut.Render(p => p.Add(c => c.Id, 7));
            await Task.Delay(300);

            Assert.Equal(1, _api.Gets);
            Assert.Equal(2, RemainingQuestions(cut));

            cut.Render(p => p.Add(c => c.Id, 8));
            await cut.WaitForStateAsync(() => _api.Gets == 2, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Disposing_the_component_stops_the_timer()
        {
            var ended = 0;
            var cut = Render(timer: true, onEnd: _ => ended++);
            await cut.WaitForStateAsync(() => _api.Gets == 1, TimeSpan.FromSeconds(5));
            await Task.Delay(500);

            await DisposeComponentsAsync();                  // mintha elnavigált volna a felhasználó
            await Task.Delay(4500);

            Assert.Equal(0, _api.Results);
            Assert.Equal(0, ended);
        }

        [Fact]
        public async Task Check_releases_the_busy_lock_so_next_still_works()
        {
            _api.QuestionTime = 30;
            var cut = Render(timer: false);
            await cut.WaitForStateAsync(() => _api.Gets == 1, TimeSpan.FromSeconds(5));

            await Call(cut, "Check");
            await Call(cut, "Next");

            Assert.Equal(1, _api.Checks);
            Assert.Equal(1, _api.Results);
        }
    }
}
