using System.Net;
using System.Text;
using System.Text.Json;

using TeszterLibrary.Model;

namespace TeszterTests.Helpers
{
    /// <summary>
    /// A teszt-kitöltés API-ja (TUser/Test, TUser/Result, TUser/Check) hamisítva.
    /// A számlálókkal ellenőrizhető, hányszor és milyen idővel hívta a komponens a szervert.
    /// </summary>
    public sealed class TestRunApiHandler : HttpMessageHandler
    {
        private int _gets;
        private int _results;
        private int _checks;

        public int Gets => _gets;
        public int Results => _results;
        public int Checks => _checks;
        public List<int> ReportedTimes { get; } = new();

        public int QuestionCount { get; set; } = 3;
        public int QuestionTime { get; set; } = 2;
        public int LatencyMs { get; set; } = 80;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(LatencyMs, cancellationToken);
            var path = request.RequestUri!.AbsolutePath;

            if (request.Method == HttpMethod.Get && path.StartsWith("/TUser/Test/"))
            {
                Interlocked.Increment(ref _gets);
                var questions = Enumerable.Range(1, QuestionCount).Select(i => new
                {
                    testQuestionId = i,
                    questionText = $"kerdes{i}",
                    questionType = (int)QuestionType.SC,
                    score = 1,
                    time = QuestionTime,
                    answers = new[] { new { answerId = i * 10, text = "a", sign = false } },
                });
                return Json(JsonSerializer.Serialize(questions));
            }

            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            if (path.StartsWith("/TUser/Result/"))
            {
                Interlocked.Increment(ref _results);
                lock (ReportedTimes) ReportedTimes.Add(JsonDocument.Parse(body).RootElement.GetProperty("time").GetInt32());
                return Json(body);
            }
            if (path == "/TUser/Check")
            {
                Interlocked.Increment(ref _checks);
                return Json(body);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private static HttpResponseMessage Json(string json)
            => new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
