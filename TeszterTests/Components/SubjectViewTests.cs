using System.Net;
using System.Reflection;

using Bunit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Extensions;
using TeszterLibrary.Model;
using TeszterLibrary.Services;

using TeszterTests.Helpers;

using TeszterWeb.Components.Pages;

namespace TeszterTests.Components
{
    /// <summary>
    /// A tantárgyak oldala (/subject). Régen: a törlés hiba esetén is eltüntette a sort, és ha a lista nem töltődött be,
    /// a táblázat renderelése NullReferenceException-nel elszállt.
    /// </summary>
    public class SubjectViewTests : BunitContext
    {
        private readonly StubHttpHandler _http = new();
        private readonly ToastRecorder _toasts;

        public SubjectViewTests()
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
            Services.AddScoped<SubjectServices>();
            Services.AddSingleton(ToastRecorder.Create(out _toasts));
        }

        private void Respond(HttpStatusCode deleteStatus)
            => _http.Respond = request => request.Method == HttpMethod.Delete
                ? StubHttpHandler.Json(deleteStatus, deleteStatus == HttpStatusCode.OK ? "true" : null)
                : StubHttpHandler.Json(HttpStatusCode.OK, """[{"id":1,"name":"Matek"},{"id":2,"name":"Fizika"}]""");

        private static List<Subject> Subjects(IRenderedComponent<SubjectView> cut)
            => (List<Subject>)typeof(SubjectView).GetField("subjects", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(cut.Instance)!;

        private static Task Delete(IRenderedComponent<SubjectView> cut, Subject subject)
            => cut.InvokeAsync(() => (Task)typeof(SubjectView).GetMethod("Delete", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(cut.Instance, [subject])!);

        [Fact]
        public async Task A_successful_delete_removes_the_row()
        {
            Respond(HttpStatusCode.OK);
            var cut = Render<SubjectView>();
            cut.WaitForState(() => Subjects(cut).Count == 2);

            // Valódi kattintás a "Fizika" sor Törlés gombján (ahogy a felhasználó teszi).
            var deleteButtons = cut.FindAll("fluent-button").Where(b => b.TextContent.Trim() == "Törlés").ToList();
            Assert.Equal(2, deleteButtons.Count);
            await deleteButtons[1].ClickAsync(new());

            Assert.Equal(["Matek"], Subjects(cut).Select(s => s.Name));
            Assert.Contains(_http.Requests, r => r.Method == "DELETE" && r.Uri.EndsWith("/Subject/2"));
            Assert.DoesNotContain("Fizika", cut.Markup);
        }

        [Fact]
        public async Task A_failed_delete_keeps_the_row_and_reports_the_error()
        {
            Respond(HttpStatusCode.Conflict);
            var cut = Render<SubjectView>();
            cut.WaitForState(() => Subjects(cut).Count == 2);

            await Delete(cut, Subjects(cut).Single(s => s.Id == 2));

            Assert.Equal(2, Subjects(cut).Count);
            Assert.Contains(_toasts.Messages, m => m.StartsWith("ShowError:Nem sikerült törölni a(z) Fizika tantárgyat."));
        }

        [Fact]
        public void An_unavailable_api_leaves_an_empty_list_instead_of_crashing()
        {
            _http.Respond = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);

            var cut = Render<SubjectView>();
            cut.WaitForAssertion(() => Assert.Contains("ShowInfo:Nincs tantárgy", _toasts.Messages));

            Assert.Empty(Subjects(cut));
            Assert.Contains("Tárolt tantárgyak", cut.Markup);
        }
    }
}
