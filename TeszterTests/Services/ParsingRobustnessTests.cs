using System.Text.RegularExpressions;

using Bunit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Components.Teszt;
using TeszterLibrary.Extensions;
using TeszterLibrary.Services;
using TeszterLibrary.ViewModel;

using TeszterTests.Helpers;
using TeszterTests.Security;

using TeszterWeb.Components.Pages.Teacher;
using TeszterWeb.Data;

namespace TeszterTests.Services
{
    /// <summary>
    /// Időformázás és bemenet-feldolgozás. Régen: 3600 mp "60 p : 0 mp" lett, nem volt nullával kitöltés, és a
    /// ~45 Guid.Parse/int.Parse/Convert.ToInt32 hívás egy üres vagy hibás értéken (pl. frissítés után üres UserId,
    /// kézzel átírt URL, hiányos Graph-adat) kezeletlen kivétellel megszakította a Blazor-kapcsolatot.
    /// </summary>
    public class ParsingRobustnessTests
    {
        [Theory]
        [InlineData(0, "0 mp")]
        [InlineData(59, "59 mp")]
        [InlineData(60, "1 p : 00 mp")]
        [InlineData(65, "1 p : 05 mp")]
        [InlineData(3599, "59 p : 59 mp")]
        [InlineData(3600, "1 ó : 00 p : 00 mp")]
        [InlineData(3661, "1 ó : 01 p : 01 mp")]
        [InlineData(7322, "2 ó : 02 p : 02 mp")]
        [InlineData(-1, "")]
        public void Seconds_are_formatted_with_correct_boundaries_and_padding(int seconds, string expected)
        {
            Assert.Equal(expected, seconds.ToTime());
        }

        [Theory]
        [InlineData("3600", "1 ó : 00 p : 00 mp")]
        [InlineData(" 65 ", "1 p : 05 mp")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("abc", "")]
        [InlineData("99999999999", "")]
        public void Text_seconds_are_formatted_or_ignored(string? value, string expected)
        {
            Assert.Equal(expected, value.ToTime());
        }

        [Theory]
        [InlineData("12", 12)]
        [InlineData("-3", -3)]
        [InlineData("", null)]
        [InlineData(null, null)]
        [InlineData("1.5", null)]
        [InlineData("1e3", null)]
        [InlineData("١٢", null)]            // nem ASCII számjegy
        [InlineData("2147483648", null)]    // túlcsordulás
        public void ToIntOrNull_never_throws(string? value, int? expected)
        {
            Assert.Equal(expected, value.ToIntOrNull());
        }

        [Fact]
        public void ToGuidOrNull_never_throws()
        {
            var id = Guid.NewGuid();

            Assert.Equal(id, id.ToString().ToGuidOrNull());
            Assert.Null("".ToGuidOrNull());
            Assert.Null(((string?)null).ToGuidOrNull());
            Assert.Null("nem-guid".ToGuidOrNull());
        }

        [Fact]
        public void GlobalData_exposes_the_user_id_only_when_it_is_valid()
        {
            var id = Guid.NewGuid();

            Assert.Null(new GlobalData().UserGuid);                       // frissítés után még üres
            Assert.Null(new GlobalData { UserId = "x" }.UserGuid);
            Assert.Equal(id, new GlobalData { UserId = id.ToString() }.UserGuid);
        }

        [Fact]
        public void Numeric_route_parameters_are_constrained()
        {
            var pages = PageAuditor.RoutablePages(typeof(GlobalData).Assembly);

            Assert.Contains("/start/{number:int}", pages.Single(p => p.Name == "Startpage").Routes);
            Assert.Contains("/TaskResult/{id:int}/{name}", pages.Single(p => p.Name == "TaskResultView").Routes);
        }

        // Őrteszt: nem literálra hívott Guid.Parse / int.Parse / Convert.ToInt32 ne kerüljön vissza.
        private static readonly Regex RawParse = new(@"\b(?:Guid|int|long|double|decimal|DateTime)\.Parse\s*\(\s*(?!"")|Convert\.To(?:Int32|Int64|Double|Decimal)\s*\(");
        private static readonly Regex Comment = new(@"@\*.*?\*@|/\*.*?\*/", RegexOptions.Singleline);

        [Theory]
        [InlineData("TeszterLibrary")]
        [InlineData("TeszterWeb")]
        public void No_unchecked_parsing_in_the_sources(string project)
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root is not null && !File.Exists(Path.Combine(root.FullName, "TeszterAll.sln"))) root = root.Parent;
            var dir = Path.Combine(root!.FullName, project);
            var sep = Path.DirectorySeparatorChar;

            var violations = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                .Where(f => (f.EndsWith(".cs") || f.EndsWith(".razor")) && !f.Contains($"{sep}obj{sep}") && !f.Contains($"{sep}bin{sep}"))
                .SelectMany(f =>
                {
                    var text = Comment.Replace(File.ReadAllText(f), m => new string('\n', m.Value.Count(c => c == '\n')));
                    return text.Split('\n')
                        .Select((line, i) => (line: Regex.Replace(line, @"//.*$", string.Empty), i))
                        .Where(x => RawParse.IsMatch(x.line))
                        .Select(x => $"{Path.GetRelativePath(dir, f)}:{x.i + 1}: {x.line.Trim()}");
                })
                .ToList();

            Assert.True(violations.Count == 0,
                "Ellenőrizetlen Parse (használd a ToGuidOrNull/ToIntOrNull-t vagy a GlobalData.UserGuid-ot):" + Environment.NewLine + string.Join(Environment.NewLine, violations));
        }

        [Theory]
        [InlineData("Guid.Parse(data.UserId)", true)]
        [InlineData("int.Parse(Number)", true)]
        [InlineData("Convert.ToInt32(t)", true)]
        [InlineData("Guid.Parse(\"57da251f-9472-46f2-b8ac-3b6aaeaece1c\")", false)]
        [InlineData("Guid.TryParse(value, out var id)", false)]
        public void The_guard_pattern_recognises_unchecked_parsing(string line, bool flagged)
        {
            Assert.Equal(flagged, RawParse.IsMatch(line));
        }
    }

    public class ParsingRobustnessComponentTests : BunitContext
    {
        public ParsingRobustnessComponentTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddFluentUIComponents();
        }

        [Fact]
        public void A_fill_in_text_with_empty_or_unknown_placeholders_renders_instead_of_crashing()
        {
            var answers = new List<AnswerView> { new() { AnswerId = 1, Text = "egy" } };

            // "{}" üres helyőrző és "{9}" nem létező válasz: régen FormatException / ArgumentOutOfRangeException.
            var cut = Render<Test_comp>(p => p.Add(c => c.Answers, answers).Add(c => c.Texts, "A {1} és {} meg {9} vége"));

            Assert.Single(cut.FindAll("fluent-text-input"));   // Fluent UI v5: FluentTextInput → <fluent-text-input>
            Assert.Contains("vége", cut.Markup);
        }

        [Fact]
        public void A_pair_question_whose_pair_members_have_the_same_sign_renders_instead_of_crashing()
        {
            Services.AddSingleton(TeszterTests.Helpers.ToastRecorder.Create(out _));
            // Régi Excel-feltöltésből: a pár mindkét tagja Right = true → Sign = true (régen: "same key" kivétel).
            var answers = new List<AnswerView>
            {
                new() { AnswerId = 20, Sequence = 1, Sign = true, Text = "futás" },
                new() { AnswerId = 21, Sequence = 1, Sign = true, Text = "egyenletes" },
                new() { AnswerId = 22, Sequence = 2, Sign = true, Text = "esés" },
                new() { AnswerId = 23, Sequence = 2, Sign = true, Text = "gyorsuló" },
            };

            var cut = Render<Test_pair>(p => p.Add(c => c.Answers, answers));
            cut.Render();

            Assert.NotNull(cut.Markup);
        }

        [Fact]
        public void The_closed_tests_page_reports_an_invalid_user_id_instead_of_crashing()
        {
            var http = new StubHttpHandler();
            Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiUrl"] = "https://api.test",
                ["TeszterApi:BaseUrl"] = "api://abc",
                ["TeszterApi:Scopes:0"] = "access_as_user",
            }).Build());
            Services.AddTeszterApiClients();
            Services.AddHttpClient(TeszterApiService.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => http);
            Services.AddScoped(_ => FakeAuthorizationHeaderProvider.Create("tok"));
            Services.AddScoped<TestServices>();
            Services.AddScoped<TestQuestionServices>();
            Services.AddScoped(_ => new GlobalData { UserId = "nem-guid" });
            // A ClosedTest üres UserId esetén a Graph-ból kéri le a felhasználót; itt az azonosító nem üres,
            // így a GraphService nem hívódik – csak a befecskendezéshez kell (a kérések a stubba mennének).
            Services.AddScoped(_ => new GraphService(
                new Microsoft.Graph.GraphServiceClient(new HttpClient(http)),
                Microsoft.Extensions.Logging.Abstractions.NullLogger<GraphService>.Instance));

            var cut = Render<ClosedTest>();

            Assert.Contains("Nincs bejelentkezve a felhasználó!", cut.Markup);
            Assert.Empty(http.Requests);
        }
    }
}
