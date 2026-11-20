using System.Net;
using System.Reflection;

using Bunit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Extensions;
using TeszterLibrary.Model;
using TeszterLibrary.Services;
using TeszterLibrary.ViewModel;

using TeszterTests.Helpers;

using TeszterWeb.Components;

namespace TeszterTests.Components
{
    /// <summary>
    /// Az órai nézet (ClassLessonHost): a tanóra-időbeosztás szerinti viselkedés, az osztály feladatának betöltése,
    /// hibatűrés. Az időt TimeProvider-en át szimuláljuk (a csengetési rendet lásd: LessonClockTests).
    /// </summary>
    public class ClassLessonHostTests : BunitContext
    {
        private sealed class FixedTime(DateTimeOffset utc) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => utc;
        }

        private readonly StubHttpHandler _http = new();
        private readonly Guid _classId = Guid.NewGuid();
        private ToastRecorder _toasts = null!;

        /// <param name="utc">A szimulált pillanat UTC-ben. 2026-11-09 hétfő, a téli időszámítás miatt az iskolai idő UTC+1.</param>
        private void Arrange(string utc, params Group[] extraGroups)
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
            Services.AddSingleton(GroupsServiceFactory.Create([new Group(_classId, "8.a", "x") { HasLessons = true }, new Group(Guid.NewGuid(), "8.b", null) { HasLessons = true }, .. extraGroups]));
            Services.AddSingleton(ToastRecorder.Create(out _toasts));
            Services.AddSingleton<TimeProvider>(new FixedTime(DateTimeOffset.Parse(utc)));

            _http.Respond = request => request.RequestUri!.AbsolutePath switch
            {
                "/Math/Groups" => StubHttpHandler.Json(HttpStatusCode.OK, $"[\"{_classId}\"]"),
                var path when path.StartsWith("/Math/Class/") => StubHttpHandler.Json(HttpStatusCode.OK,
                    """{"taskId":1,"title":"t","operations":[{"operationId":7},{"operationId":8}]}"""),
                "/Math/ClassResult" => StubHttpHandler.Json(HttpStatusCode.OK, request.Content!.ReadAsStringAsync().Result),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound),
            };
        }

        private IRenderedComponent<ClassLessonHost> Render(bool teacher)
            => Render<ClassLessonHost>(p => p.Add(c => c.IsTeacher, teacher));

        // A komponens privát tagjait reflexióval érjük el: a viselkedést teszteljük, nem a megjelenítést.
        private static T Field<T>(IRenderedComponent<ClassLessonHost> cut, string name)
            => (T)typeof(ClassLessonHost).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(cut.Instance)!;

        private static Task Call(IRenderedComponent<ClassLessonHost> cut, string method, params object[] args)
            => cut.InvokeAsync(() => (Task)typeof(ClassLessonHost)
                .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(cut.Instance, args)!);

        [Fact]
        public void Only_groups_with_lessons_are_offered_and_the_api_is_asked_for_the_groups()
        {
            Arrange("2026-11-09T09:00:00Z");

            var cut = Render(teacher: false);

            Assert.Equal(2, Field<List<Group>>(cut, "Groups").Count);
            Assert.Contains(_http.Requests, r => r.Uri.EndsWith("/Math/Groups"));
        }

        [Fact]
        public void Groups_reported_by_the_api_are_marked_and_offered()
        {
            var apiOnly = new Group(Guid.NewGuid(), "8.c", null);              // a helyi nyilvántartásban még nincs jelölve
            var noLessons = new Group(Guid.NewGuid(), "8.d", null);
            Arrange("2026-11-09T09:00:00Z", apiOnly, noLessons);
            _http.Respond = request => request.RequestUri!.AbsolutePath == "/Math/Groups"
                ? StubHttpHandler.Json(HttpStatusCode.OK, $"[\"{_classId}\",\"{apiOnly.Id}\"]")
                : new HttpResponseMessage(HttpStatusCode.NotFound);

            var cut = Render(teacher: false);
            cut.WaitForState(() => Field<List<Group>>(cut, "Groups").Count == 3, TimeSpan.FromSeconds(5));   // az API-hívás után bővül

            var offered = Field<List<Group>>(cut, "Groups").Select(g => g.Name).Order();
            Assert.Equal(["8.a", "8.b", "8.c"], offered);                    // régen a jelölő ciklus halott kód volt, a 8.c kimaradt
        }

        [Theory]
        [InlineData(true)]     // tanári nézet
        [InlineData(false)]    // névtelen (kivetítős) nézet
        public async Task During_a_lesson_the_tasks_are_prepared_for_the_selected_class(bool teacher)
        {
            Arrange("2026-11-09T09:00:00Z");                       // hétfő 10:00 iskolai idő
            var cut = Render(teacher);

            await Call(cut, "SelectedClass", _classId.ToString());

            Assert.True(Field<bool>(cut, "IsLessonTime"));
            var recives = Field<List<ClassRecive>>(cut, "recives");
            Assert.Equal(2, recives.Count);
            Assert.All(recives, r =>
            {
                Assert.Equal(_classId, r.ClassId);
                Assert.Equal(Guid.Empty, r.StudentId);
            });
        }

        [Fact]
        public async Task Before_the_first_lesson_the_page_waits_with_the_remaining_time()
        {
            Arrange("2026-11-09T06:30:00Z");                       // 07:30 iskolai idő
            var cut = Render(teacher: false);

            await Call(cut, "SelectedClass", _classId.ToString());

            Assert.False(Field<bool>(cut, "IsLessonTime"));
            Assert.Equal(TimeSpan.FromMinutes(30), Field<TimeSpan>(cut, "remainingTime"));
        }

        [Fact]
        public async Task On_a_weekend_the_task_is_shown_with_a_message()
        {
            Arrange("2026-11-14T10:00:00Z");                       // szombat
            var cut = Render(teacher: false);

            await Call(cut, "SelectedClass", _classId.ToString());

            Assert.True(Field<bool>(cut, "IsLessonTime"));
            Assert.Contains("ShowInfo:Hétvége van, jó gyakorlást!", _toasts.Messages);
        }

        [Fact]
        public async Task A_failing_api_does_not_crash_the_classroom_page()
        {
            Arrange("2026-11-09T09:00:00Z");
            _http.Respond = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var cut = Render(teacher: true);

            await Call(cut, "SelectedClass", _classId.ToString());      // nem dobhat kivételt

            Assert.Contains("ShowInfo:Nem található művelet", _toasts.Messages);
            Assert.NotNull(Field<TaskView>(cut, "taskView").Operations);   // nem ragad a betöltő állapotban
        }

        [Fact]
        public async Task An_invalid_class_id_is_reported_instead_of_thrown()
        {
            Arrange("2026-11-09T09:00:00Z");
            var cut = Render(teacher: true);

            await Call(cut, "SelectedClass", "nem-guid");

            Assert.Contains("ShowError:Érvénytelen osztály azonosító.", _toasts.Messages);
        }

        [Fact]
        public async Task An_empty_selection_asks_for_a_class()
        {
            Arrange("2026-11-09T09:00:00Z");
            var cut = Render(teacher: true);

            await Call(cut, "SelectedClass", "");

            Assert.Contains("ShowMention:Kérlek válasz osztályt!", _toasts.Messages);
        }

        [Fact]
        public async Task CheckClass_normalises_decimal_commas_and_tolerates_missing_results()
        {
            Arrange("2026-11-09T09:00:00Z");
            var cut = Render(teacher: true);
            await Call(cut, "SelectedClass", _classId.ToString());
            var answers = new List<ClassRecive> { new() { OperationId = 7, Result = "1,5" }, new() { OperationId = 8, Result = null! } };

            await Call(cut, "CheckClass", answers);

            var body = _http.Requests.Last(r => r.Uri.EndsWith("/Math/ClassResult")).Body;
            Assert.Contains("\"result\":\"1.5\"", body);
            Assert.True(Field<bool>(cut, "IsChecked"));
        }
    }
}
