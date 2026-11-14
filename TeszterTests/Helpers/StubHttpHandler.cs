using System.Net;
using System.Text;

namespace TeszterTests.Helpers
{
    /// <summary>HTTP-kéréseket rögzítő és előre megadott választ adó kezelő (nincs hálózat).</summary>
    public sealed class StubHttpHandler : HttpMessageHandler
    {
        public sealed record Seen(string Method, string Uri, string? Authorization, string Body);

        public List<Seen> Requests { get; } = new();
        public Func<HttpRequestMessage, HttpResponseMessage> Respond { get; set; } = _ => new HttpResponseMessage(HttpStatusCode.OK);

        public void RespondWith(HttpStatusCode status, string? json = null)
            => Respond = _ => Json(status, json);

        public static HttpResponseMessage Json(HttpStatusCode status, string? json)
        {
            var response = new HttpResponseMessage(status);
            if (json is not null) response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            lock (Requests)
            {
                Requests.Add(new Seen(request.Method.Method, request.RequestUri!.ToString(), request.Headers.Authorization?.ToString(), body));
            }
            await Task.Yield();
            return Respond(request);
        }
    }
}
