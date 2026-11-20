using System.Reflection;

using TeszterLibrary.Services;

namespace TeszterTests.Security
{
    /// <summary>
    /// Őrtesztek a jogosultságkezelésre. Nincs FallbackPolicy (az blokkolná a Blazor-hubot a névtelen Indexnél),
    /// ezért a védelem oldalszintű attribútumokon múlik (Components/Pages/_Imports.razor és a mappánkénti _Imports.razor).
    /// Ha valaki új oldalt tesz a Pages mappán kívülre és nem jelöli meg, ezek a tesztek elbuknak.
    /// </summary>
    public class PageAuthorizationTests
    {
        // Ismert, védtelen maradványok. Jelenleg nincs ilyen (az Authentication WASM-maradványt töröltük);
        // ha mégis kell egy átmeneti kivétel, ide kerüljön (a "Known_leftovers_still_exist" teszt szól, ha elavult).
        private static readonly string[] KnownUnprotected = [];

        // Törölt demó- és maradványoldalak: ne kerüljenek vissza.
        private static readonly string[] RemovedPages = ["Authentication", "Counter", "Editor", "TestPage", "Weather", "SubjectCRUD"];

        // Az egyetlen szándékosan nyilvános oldal a kezdőoldal és a hibaoldal.
        private static readonly string[] AnonymousPages = { "Error", "Index" };

        private static readonly IReadOnlyList<PageInfo> Pages =
            PageAuditor.RoutablePages(Assembly.Load("TeszterWeb"), typeof(ApiClientBase).Assembly);

        [Fact]
        public void The_scan_finds_the_real_pages()
        {
            Assert.True(Pages.Count > 10, $"Gyanúsan kevés oldal: {Pages.Count}");
            Assert.Contains(Pages, p => p.Name == "Index" && p.Routes.Contains("/"));
            Assert.Contains(Pages, p => p.Name == "Home");
        }

        [Fact]
        public void Every_routable_page_declares_its_authorization()
        {
            var offenders = Pages
                .Where(p => !p.HasAuthorize && !p.AllowAnonymous && !KnownUnprotected.Contains(p.Name))
                .Select(p => p.Type.FullName)
                .ToList();

            Assert.True(offenders.Count == 0, "Nincs [Authorize] / [AllowAnonymous] ezeken: " + string.Join(", ", offenders));
        }

        [Fact]
        public void Only_the_intended_pages_are_anonymous()
        {
            var anonymous = Pages.Where(p => p.AllowAnonymous).Select(p => p.Name).Order().ToArray();

            Assert.Equal(AnonymousPages, anonymous);
        }

        [Fact]
        public void Teacher_and_assignment_pages_require_the_Teacher_role()
        {
            var teacherPages = Pages.Where(p => p.Namespace.Contains(".Pages.Teacher") || p.Namespace.Contains(".Pages.Feladat") || p.Name == "UserManagment").ToList();

            Assert.NotEmpty(teacherPages);
            Assert.All(teacherPages, p => Assert.True(p.Roles.Contains("Teacher"), $"{p.Type.FullName} nem kér Teacher szerepkört"));
        }

        [Fact]
        public void Student_pages_require_the_Student_role()
        {
            var studentPages = Pages.Where(p => p.Namespace.Contains(".Pages.Student")).ToList();

            Assert.NotEmpty(studentPages);
            Assert.All(studentPages, p => Assert.True(p.Roles.Contains("Student"), $"{p.Type.FullName} nem kér Student szerepkört"));
        }

        [Fact]
        public void Every_route_belongs_to_exactly_one_page()
        {
            // Két azonos útvonal (pl. régen SubjectCRUD és SubjectView: "/subject") futásidőben AmbiguousMatchException.
            var duplicates = Pages
                .SelectMany(p => p.Routes.Select(r => (Route: r.TrimEnd('/').ToLowerInvariant(), Page: p.Type.FullName)))
                .GroupBy(x => x.Route)
                .Where(g => g.Count() > 1)
                .Select(g => $"{g.Key}: {string.Join(", ", g.Select(x => x.Page))}")
                .ToList();

            Assert.True(duplicates.Count == 0, "Ütköző útvonalak: " + string.Join("; ", duplicates));
        }

        [Fact]
        public void Removed_demo_pages_stay_removed()
        {
            Assert.DoesNotContain(Pages, p => RemovedPages.Contains(p.Name));
        }

        [Fact]
        public void Known_leftovers_still_exist()
        {
            Assert.All(KnownUnprotected, name => Assert.True(Pages.Any(p => p.Name == name), $"A(z) {name} oldal már nincs meg: töröld a KnownUnprotected listából."));
        }
    }
}
