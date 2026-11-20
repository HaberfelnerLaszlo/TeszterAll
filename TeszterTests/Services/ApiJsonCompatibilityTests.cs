using System.Net;

using TeszterLibrary.Model;

using TeszterTests.Helpers;

namespace TeszterTests.Services
{
    /// <summary>
    /// Az API-válaszok olvasásának viselkedése. Ezek a tesztek a Newtonsoft.Json-os változatot rögzítik (azon is
    /// zöldek), hogy a System.Text.Json-ra váltás ne változtasson semmin: az STJ alapból szigorúbb (enum csak számként,
    /// szám nem kerülhet szöveg mezőbe, a bool csak true/false, nincs komment és záró vessző).
    /// </summary>
    public class ApiJsonCompatibilityTests
    {
        private sealed class Sample
        {
            public int Id { get; set; }
            public string Text { get; set; } = string.Empty;
            public bool Flag { get; set; }
            public QuestionType Kind { get; set; }
            public DateTime When { get; set; }
            public decimal Amount { get; set; }
            public List<int> Numbers { get; set; } = new();
            public Guid? Owner { get; set; }
        }

        private static async Task<T?> Read<T>(string json)
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, json);
            return await host.Api.Get<T>("x");
        }

        [Theory]
        [InlineData("\"SC\"", QuestionType.SC)]
        [InlineData("\"mc\"", QuestionType.MC)]       // kis-nagybetű független
        [InlineData("4", QuestionType.Open)]          // számként is
        public async Task Enums_are_read_from_names_or_numbers(string kind, QuestionType expected)
        {
            var sample = await Read<Sample>($$"""{"kind":{{kind}}}""");

            Assert.Equal(expected, sample!.Kind);
        }

        [Theory]
        [InlineData("""{"text":5}""", "5")]
        [InlineData("""{"text":true}""", "True")]
        [InlineData("""{"text":"szöveg"}""", "szöveg")]
        public async Task Numbers_and_booleans_are_accepted_in_text_fields(string json, string expected)
        {
            Assert.Equal(expected, (await Read<Sample>(json))!.Text);
        }

        [Theory]
        [InlineData("""{"flag":true}""", true)]
        [InlineData("""{"flag":"true"}""", true)]
        [InlineData("""{"flag":1}""", true)]
        [InlineData("""{"flag":0}""", false)]
        public async Task Booleans_are_read_leniently(string json, bool expected)
        {
            Assert.Equal(expected, (await Read<Sample>(json))!.Flag);
        }

        [Fact]
        public async Task Numbers_in_strings_and_case_insensitive_names_are_accepted()
        {
            var sample = await Read<Sample>("""{"ID":"42","AMOUNT":"1.5","Numbers":[1,"2"]}""");

            Assert.Equal(42, sample!.Id);
            Assert.Equal(1.5m, sample.Amount);
            Assert.Equal([1, 2], sample.Numbers);
        }

        [Fact]
        public async Task Comments_trailing_commas_and_unknown_members_are_tolerated()
        {
            var sample = await Read<Sample>("""
                {
                  // megjegyzés
                  "id": 3, /* blokk */
                  "ismeretlen": { "x": [1, 2] },
                  "numbers": [1, 2,],
                }
                """);

            Assert.Equal(3, sample!.Id);
            Assert.Equal([1, 2], sample.Numbers);
        }

        [Fact]
        public async Task Iso_dates_and_guids_are_read_and_null_stays_null()
        {
            var owner = Guid.NewGuid();
            var sample = await Read<Sample>($$"""{"when":"2026-11-09T10:15:00","owner":"{{owner}}"}""");

            Assert.Equal(new DateTime(2026, 11, 9, 10, 15, 0), sample!.When);
            Assert.Equal(owner, sample.Owner);
            Assert.Null((await Read<Sample>("""{"owner":null}"""))!.Owner);
        }

        [Fact]
        public async Task Real_models_round_trip_through_the_envelope()
        {
            var question = await Read<Question>("""
                {"isSuccess":true,"errorMessage":"","content":
                  {"id":"6f1c4f0e-8f5d-4a52-9d0f-2f0d8b1a1c11","text":"<p>2+2?</p>","qType":"MC","score":3,"time":30,
                   "answers":[{"id":1,"text":"4","right":true},{"id":2,"text":"5","right":false}]}}
                """);

            Assert.Equal(QuestionType.MC, question!.QType);
            Assert.Equal(2, question.Answers.Count);
            Assert.True(question.Answers[0].Right);

            var test = await Read<Test>("""{"id":7,"title":"Dolgozat","endTime":"2026-12-01T08:00:00","isTimer":false,"testQuestions":[{"id":1}]}""");
            Assert.Equal(("Dolgozat", false, 1), (test!.Title, test.IsTimer, test.TestQuestions.Count));
        }

        [Fact]
        public async Task Primitive_roots_are_read()
        {
            var id = Guid.NewGuid();

            Assert.True(await Read<bool>("true"));
            Assert.Equal(7, await Read<int>("7"));
            Assert.Equal("kész", await Read<string>("\"kész\""));
            var array = await Read<int[]>("[1,2,3]");
            Assert.Equal(new[] { 1, 2, 3 }, array);
            Assert.Equal([id], await Read<List<Guid>>($"[\"{id}\"]"));
            Assert.Null(await Read<Sample>("""{"isSuccess":true,"errorMessage":"","content":null}"""));
        }

        [Fact]
        public async Task A_body_that_does_not_fit_the_type_is_reported_not_thrown()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, """{"id":{"nem":"szám"}}""");

            var sample = await host.Api.Get<Sample>("x");

            Assert.Null(sample);
            Assert.StartsWith("Hiba történt!", host.Api.ErrorMessage);
        }
    }
}
