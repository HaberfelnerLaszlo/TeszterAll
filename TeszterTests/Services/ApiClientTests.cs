using System.Net;

using Microsoft.Extensions.DependencyInjection;

using TeszterLibrary.Services;
using TeszterTests.Helpers;

namespace TeszterTests.Services
{
    /// <summary>A TeszterApiService és a FeladatApiService közös ApiClientBase-e: boríték, hibák, token, névtelen útvonalak.</summary>
    public class ApiClientTests
    {
        private sealed record Item(int Id, string Text);

        [Fact]
        public async Task Get_unwraps_a_success_envelope_and_sends_bearer_token_to_the_configured_url()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, """{"isSuccess":true,"errorMessage":"","content":[{"id":1,"text":"a"}]}""");

            var items = await host.Api.Get<List<Item>>("Question");

            var item = Assert.Single(items!);
            Assert.Equal("a", item.Text);
            var request = Assert.Single(host.Handler.Requests);
            Assert.Equal("Bearer tok", request.Authorization);
            Assert.Equal("https://api.test/Question", request.Uri);
        }

        [Fact]
        public async Task Get_accepts_a_raw_array_and_keeps_date_like_text_untouched()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, """[{"id":2,"text":"2020-01-01T00:00:00 nem dátum"}]""");

            var items = await host.Api.Get<List<Item>>("Question");

            Assert.Equal("2020-01-01T00:00:00 nem dátum", Assert.Single(items!).Text);
        }

        [Fact]
        public async Task Content_that_merely_mentions_errorMessage_is_not_mistaken_for_an_envelope()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, """{"id":3,"text":"errorMessage a szövegben"}""");

            var item = await host.Api.Get<Item>("Question/3");

            Assert.Equal(3, item!.Id);
        }

        [Fact]
        public async Task A_failure_envelope_returns_null_and_raises_the_message_event()
        {
            using var host = new ApiTestHost();
            string? message = null;
            host.Api.OnMessageReceived = (_, e) => message = e.Message;
            host.Handler.RespondWith(HttpStatusCode.OK, """{"isSuccess":false,"errorMessage":"Nincs jogosultság","content":null}""");

            var result = await host.Api.Get<List<Item>>("Question");

            Assert.Null(result);
            Assert.Equal("Nincs jogosultság", message);
        }

        [Fact]
        public async Task NotFound_without_envelope_reports_the_server_as_unreachable()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.NotFound, "");

            Assert.Null(await host.Api.Get<Item>("x"));
            Assert.Equal("Nem elérhető a szerver!", host.Api.ErrorMessage);
        }

        [Fact]
        public async Task ServerError_reports_the_status_and_never_leaks_the_token_into_the_message()
        {
            using var host = new ApiTestHost();
            host.Handler.Respond = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("boom") };

            Assert.Null(await host.Api.Get<Item>("x"));
            Assert.StartsWith("500", host.Api.ErrorMessage);
            Assert.DoesNotContain("Bearer", host.Api.ErrorMessage);
            Assert.DoesNotContain("tok", host.Api.ErrorMessage.Replace("boom", ""));
        }

        [Fact]
        public async Task A_non_json_response_becomes_an_error_message_instead_of_an_exception()
        {
            using var host = new ApiTestHost();
            host.Handler.Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<html>") };

            Assert.Null(await host.Api.Get<Item>("x"));
            Assert.Contains("JSON", host.Api.ErrorMessage);
        }

        [Theory]
        [InlineData("Math/Groups")]
        [InlineData("Math/Class/6f1c4f0e-8f5d-4a52-9d0f-2f0d8b1a1c11")]
        [InlineData("Math/ClassResult")]
        public async Task Only_the_projector_view_routes_are_sent_without_a_token(string uri)
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, "[]");

            await host.Api.Get<List<Item>>(uri);

            Assert.Null(Assert.Single(host.Handler.Requests).Authorization);
        }

        // Az API ezeket már csak bejelentkezve engedi (TeszterData: MathController [Authorize]).
        [Theory]
        [InlineData("Math")]
        [InlineData("Math/Class")]                     // POST: osztályórák mentése (tanár)
        [InlineData("Math/ClassLessons/6f1c4f0e-8f5d-4a52-9d0f-2f0d8b1a1c11")]
        [InlineData("Math/Start/6f1c4f0e-8f5d-4a52-9d0f-2f0d8b1a1c11")]
        [InlineData("Math/Result")]
        [InlineData("Math/ResultStudent/6f1c4f0e-8f5d-4a52-9d0f-2f0d8b1a1c11")]
        [InlineData("Math/Class/")]
        [InlineData("Math/GroupsX")]
        public async Task Other_math_routes_carry_the_users_token(string uri)
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, "[]");

            await host.Api.Get<List<Item>>(uri);

            Assert.Equal("Bearer tok", Assert.Single(host.Handler.Requests).Authorization);
        }

        [Fact]
        public async Task Post_sends_camelCase_json()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, """{"id":5,"text":"x"}""");

            await host.Api.Post("Question", new Item(5, "x"));

            Assert.Equal("""{"id":5,"text":"x"}""", Assert.Single(host.Handler.Requests).Body);
        }

        [Fact]
        public async Task Delete_treats_an_empty_204_as_success()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.NoContent);

            Assert.True(await host.Api.Delete("Question/5"));
        }

        [Fact]
        public void Missing_api_scope_configuration_fails_fast_with_a_clear_message()
        {
            using var host = new ApiTestHost(config: new() { ["TeszterApi:BaseUrl"] = null });

            var ex = Assert.Throws<InvalidOperationException>(() => host.Api);
            Assert.Contains("TeszterApi:BaseUrl", ex.Message);
        }

        // ---------------------------------------------------------------- FeladatApiService

        [Fact]
        public async Task Feladat_Put_and_Delete_treat_an_empty_2xx_as_success()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.NoContent);

            Assert.True(await host.Feladat.Put("tanulo/1", new Item(1, "a")));
            Assert.True(await host.Feladat.Delete("tanulo/1"));
        }

        [Fact]
        public async Task Feladat_does_not_send_a_token()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, "[]");

            await host.Feladat.Get<List<Item>>("tanulo");

            Assert.Null(Assert.Single(host.Handler.Requests).Authorization);
        }

        [Fact]
        public async Task Feladat_Get_with_a_failure_envelope_returns_null_and_sets_the_error_message()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, """{"isSuccess":false,"errorMessage":"hiba","content":null}""");

            Assert.Null(await host.Feladat.Get<Item>("x"));
            Assert.Equal("hiba", host.Feladat.ErrorMessage);
        }

        [Fact]
        public async Task Feladat_PostFile_posts_multipart_to_the_upload_url_from_configuration()
        {
            using var host = new ApiTestHost(config: new()
            {
                ["FeladatApi:BaseUrl"] = "https://feladat.test/",
                ["FeladatApi:UploadUrl"] = "https://upload.test/upload",
            });
            host.Handler.RespondWith(HttpStatusCode.OK);

            var ok = await host.Feladat.PostFileAsync(new MemoryStream(new byte[] { 1, 2, 3 }), "a.xlsx");

            Assert.True(ok);
            var request = Assert.Single(host.Handler.Requests);
            Assert.Equal("https://upload.test/upload", request.Uri);
            Assert.Contains("filename=a.xlsx", request.Body);
        }

        // ---------------------------------------------------------------- tokenek elszigetelése

        [Fact]
        public async Task Tokens_never_mix_between_two_users_under_parallel_load()
        {
            using var host = new ApiTestHost();
            host.Handler.RespondWith(HttpStatusCode.OK, "[]");
            var userA = host.CreateApiFor("TOKEN-A");
            var userB = host.CreateApiFor("TOKEN-B");

            await Task.WhenAll(Enumerable.Range(0, 400).Select(i =>
                i % 2 == 0 ? userA.Get<List<Item>>($"User/A{i}") : userB.Get<List<Item>>($"User/B{i}")));

            Assert.Equal(400, host.Handler.Requests.Count);
            Assert.All(host.Handler.Requests, r =>
            {
                var expected = r.Uri.Contains("/A") ? "Bearer TOKEN-A" : "Bearer TOKEN-B";
                Assert.Equal(expected, r.Authorization);
            });
        }
    }
}
