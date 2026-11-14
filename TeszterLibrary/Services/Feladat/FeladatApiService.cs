using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TeszterLibrary.Services.Feladat
{
    /// <summary>
    /// A Feladat API kliense. Jelenleg NEM küld hitelesítést (mint eddig sem): ha az API
    /// tanulói adatot ad vissza, érdemes tokenhez kötni (felülírható a GetAuthorizationAsync).
    /// </summary>
    public class FeladatApiService : ApiClientBase
    {
        public const string HttpClientName = "FeladatApi";

        private readonly string _uploadPath;

        public FeladatApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<FeladatApiService> logger)
            : base(httpClientFactory.CreateClient(HttpClientName), logger)
        {
            // Abszolút URL is megadható (pl. külön feltöltő szolgáltatás), alapértelmezés: <BaseUrl>/upload
            _uploadPath = configuration["FeladatApi:UploadUrl"] ?? "upload";
        }

        public async Task<Tv?> Get<Tv>(string uri)
            => (await SendAsync<Tv>(HttpMethod.Get, uri)).Value;

        public async Task<T?> Post<T>(string uri, T t)
            => (await SendAsync<T>(HttpMethod.Post, uri, t)).Value;

        public async Task<bool> Put<T>(string uri, T t)
            => (await SendAsync<object>(HttpMethod.Put, uri, t)).IsSuccess;

        public async Task<bool> Delete(string uri)
            => (await SendAsync<object>(HttpMethod.Delete, uri)).IsSuccess;

        public async Task<bool> PostFileAsync(Stream fileStream, string fileName = "upload.bin", CancellationToken ct = default)
        {
            ErrorMessage = string.Empty;
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StreamContent(fileStream), "file", fileName);
                using var response = await Http.PostAsync(_uploadPath, content, ct);
                if (response.IsSuccessStatusCode) return true;
                ErrorMessage = $"{(int)response.StatusCode} {response.ReasonPhrase}";
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorMessage = "A feltöltés nem sikerült: " + e.Message;
            }
            return false;
        }
    }
}
