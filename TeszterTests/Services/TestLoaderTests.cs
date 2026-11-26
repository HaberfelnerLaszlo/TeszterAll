using System.Net;
using System.Security.Claims;

using Bunit;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Extensions;
using TeszterLibrary.Services;

using TeszterTests.Helpers;

using TeszterWeb.Components.Pages.Student;
using TeszterWeb.Components.Pages.Teacher;
using TeszterWeb.Data;
using TeszterWeb.Services;

namespace TeszterTests.Services
{
    /// <summary>
    /// A tesztoldalak azonosító alapú betöltése. Régen az adat a GlobalData-n át jött az előző oldaltól: frissítés (F5)
    /// után a tesztírás időzítő és beállítások nélkül indult, az eredményoldal üres volt, a tanári szerkesztés pedig
    /// üres tesztet nyitott meg. A diák csak a saját tesztjét nyithatja meg (oid claim alapján).
    /// </summary>
    public class TestLoaderTests : BunitContext
    {
        private sealed class FixedAuth(ClaimsPrincipal user) : AuthenticationStateProvider
        {
            public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(user));
        }

        private readonly StubHttpHandler _http = new();
        private readonly Guid _me = Guid.NewGuid();
        private ToastRecorder _toasts = null!;

        // A diák (TestUser 41) nem kész tesztje időzítővel; a 42-es kész; a 99-es másé.
        private const string MyOpenTests = """[{"id":5,"title":"Törtek","time":90,"isTimer":true,"isCheck":true,"isPractice":false,"shuffleQuestion":false,"testUserId":41,"endTime":"2099-01-01T00:00:00"}]""";
        private const string MyReadyTests = """[{"title":"Régi","testUserId":42}]""";
        private const string Questions = """[{"testQuestionId":1,"questionText":"<p>2+2?</p>"}]""";

        private void Arrange(ClaimsPrincipal? user = null)
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
            Services.AddScoped<TestServices>();
            Services.AddScoped<TestUserServices>();
            Services.AddScoped<TestQuestionServices>();
            Services.AddScoped<TestQuestionUserServices>();
            Services.AddScoped<GlobalData>();   // üres, mint frissítés után
            Services.AddSingleton(ToastRecorder.Create(out _toasts));
            Services.AddSingleton<AuthenticationStateProvider>(new FixedAuth(user ?? new ClaimsPrincipal(new ClaimsIdentity([new Claim("oid", _me.ToString())], "test"))));
            Services.AddScoped<TestLoader>();

            _http.Respond = request => request.RequestUri!.AbsolutePath switch
            {
                var p when p == $"/TUser/Tests/{_me}" => StubHttpHandler.Json(HttpStatusCode.OK, MyOpenTests),
                var p when p == $"/TUser/Ready/{_me}" => StubHttpHandler.Json(HttpStatusCode.OK, MyReadyTests),
                var p when p.StartsWith("/TUser/Test/") => StubHttpHandler.Json(HttpStatusCode.OK, Questions),
                "/Test/7" => StubHttpHandler.Json(HttpStatusCode.OK, """{"id":7,"title":"Dolgozat","testQuestions":[],"testUsers":[]}"""),
                "/Tq/Test/7" => StubHttpHandler.Json(HttpStatusCode.OK, """[{"id":1,"testId":7},{"id":2,"testId":7}]"""),
                "/TUser/Member/7" => StubHttpHandler.Json(HttpStatusCode.OK, """[{"id":3,"testId":7,"userId":"6f1c4f0e-8f5d-4a52-9d0f-2f0d8b1a1c11"}]"""),
                "/Tqu/Result/7" => StubHttpHandler.Json(HttpStatusCode.OK, "[]"),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound),
            };
        }

        private TestLoader Loader => Services.GetRequiredService<TestLoader>();

        [Fact]
        public async Task A_student_test_keeps_its_settings_when_loaded_by_id()
        {
            Arrange();

            var loaded = await Loader.LoadTestForStudentAsync(41);

            Assert.True(loaded.Ok);
            var test = loaded.Value!;
            Assert.Equal(("Törtek", 90, true, true, false, false), (test.Title, test.Time, test.IsTimer, test.IsCheck, test.IsPractice, test.ShuffleQuestion));
        }

        [Theory]
        [InlineData(99)]     // más diák tesztje
        [InlineData(42)]     // már kész: nem indítható újra
        public async Task A_test_that_is_not_the_students_open_test_cannot_be_started(int testUserId)
        {
            Arrange();

            var loaded = await Loader.LoadTestForStudentAsync(testUserId);

            Assert.Equal(TestLoader.NotYours, loaded.Error);
        }

        [Fact]
        public async Task Without_an_object_id_claim_nothing_is_loaded()
        {
            Arrange(new ClaimsPrincipal(new ClaimsIdentity()));

            var loaded = await Loader.LoadTestForStudentAsync(41);

            Assert.False(loaded.Ok);
            Assert.Empty(_http.Requests);
        }

        [Theory]
        [InlineData(42)]     // kész teszt
        [InlineData(41)]     // folyamatban lévő (pl. gyakorló) teszt
        public async Task A_student_sees_the_results_of_their_own_test(int testUserId)
        {
            Arrange();

            var loaded = await Loader.LoadResultsForStudentAsync(testUserId);

            Assert.True(loaded.Ok);
            Assert.Equal("<p>2+2?</p>", Assert.Single(loaded.Value!).QuestionText);
        }

        [Fact]
        public async Task Someone_elses_results_are_not_even_requested()
        {
            Arrange();

            var loaded = await Loader.LoadResultsForStudentAsync(99);

            Assert.Equal(TestLoader.NotYours, loaded.Error);
            Assert.DoesNotContain(_http.Requests, r => r.Uri.Contains("/TUser/Test/99"));
        }

        // A Teams-link (/start/{tesztId}) a diák saját listájából dolgozik; a teljes teszt (Test/{id}) tanári végpont lett.
        [Fact]
        public async Task The_start_link_finds_the_students_own_open_test()
        {
            Arrange();

            var found = await Loader.FindStudentTestAsync(5, new DateTime(2026, 11, 21));

            Assert.True(found.Ok);
            Assert.Equal(41, found.Value!.TestUserId);
            Assert.DoesNotContain(_http.Requests, r => r.Uri.Contains("/Test/5"));     // a teljes tesztet nem kéri le
        }

        [Theory]
        [InlineData(6, "2026-11-21", "nem neked adták ki")]     // nincs a diák listájában
        [InlineData(5, "2100-01-01", "lejárt")]
        [InlineData(0, "2026-11-21", "Nincs ilyen teszt")]
        public async Task The_start_link_explains_why_a_test_cannot_be_opened(int testId, string today, string reason)
        {
            Arrange();

            var found = await Loader.FindStudentTestAsync(testId, DateTime.Parse(today));

            Assert.False(found.Ok);
            Assert.Contains(reason, found.Error);
        }

        [Fact]
        public async Task A_teacher_test_is_completed_with_its_questions_and_users()
        {
            Arrange();

            var loaded = await Loader.LoadTestForTeacherAsync(7, withQuestions: true, withUsers: true);

            Assert.True(loaded.Ok);
            Assert.Equal(2, loaded.Value!.TestQuestions.Count);
            Assert.Single(loaded.Value.TestUsers);
            Assert.False((await Loader.LoadTestForTeacherAsync(8)).Ok);     // nem létező teszt
            Assert.False((await Loader.LoadTestForTeacherAsync(0)).Ok);
        }

        [Fact]
        public async Task After_a_refresh_the_test_page_still_shows_the_timer_and_settings()
        {
            Arrange();

            var cut = Render<TestRuning>(p => p.Add(c => c.Id, 41));

            await cut.WaitForAssertionAsync(() => Assert.Contains("A teszt elkezdése", cut.Markup));
            Assert.Contains("Törtek", cut.Markup);
            Assert.Contains("1 p : 30 mp", cut.Markup);                       // régen: időzítő nélkül indult
            Assert.Contains("ellenőrizheted a válaszaidat", cut.Markup);
        }

        [Fact]
        public async Task A_foreign_test_page_shows_an_error_instead_of_the_test()
        {
            Arrange();

            var cut = Render<TestRuning>(p => p.Add(c => c.Id, 99));

            await cut.WaitForAssertionAsync(() => Assert.Contains(TestLoader.NotYours, cut.Markup));
            Assert.DoesNotContain("A teszt elkezdése", cut.Markup);
        }

        [Fact]
        public async Task After_a_refresh_the_teacher_result_page_still_loads_the_test()
        {
            Arrange();

            var cut = Render<ResultTest>(p => p.Add(c => c.TestId, 7));

            await cut.WaitForAssertionAsync(() => Assert.Contains("Dolgozat teszt eredményei", cut.Markup));
            Assert.Contains(_http.Requests, r => r.Uri.EndsWith("/Tq/Test/7"));
        }

        [Fact]
        public void GlobalData_no_longer_carries_tests_between_pages()
        {
            var properties = typeof(GlobalData).GetProperties().Select(p => p.Name).ToList();

            Assert.DoesNotContain("Test", properties);
            Assert.DoesNotContain("TestResults", properties);
        }
    }
}
