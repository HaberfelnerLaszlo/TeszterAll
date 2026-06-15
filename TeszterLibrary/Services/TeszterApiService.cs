using Microsoft.Identity.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;

using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using TeszterLibrary.Events;
using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
    public class TeszterApiService(IAuthorizationHeaderProvider authorizationHeaderProvider)
    {
        public string ErrorMessage { get; set; } = String.Empty;
        private static readonly HttpClient client = new();
        private static readonly string[] scopes = ["api://6091ad40-f274-4e3a-813b-a9498817fd69/access_as_user"];
        public EventHandler<MessageEvent> OnMessageReceived { get; set; }
        //protected static string URI = "https://teszter-api.haberfelner.hu/";
        //protected static string URI = "http://localhost:7025/";
        protected static string URI = "https://localhost:44308/"; //ISS
        //protected static string URI = "https://z8m54ndx-7026.euw.devtunnels.ms"; //távoli
        //protected static string URI = "http://192.168.1.69:7025"; //laptop
        private string? _token;
        private readonly DateTime _tokenExpiration;
        private async Task CreateAuthorizationHeaderForUserAsync()
        {
                _token = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
         
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _token.Substring(6));
        }

        public async Task<Tv?> Get<Tv>(string uri)
        {
            try
            {
                client.BaseAddress ??= new Uri(URI);
                if (string.IsNullOrEmpty(_token) && !uri.Contains("Math"))
                {
                    await CreateAuthorizationHeaderForUserAsync();
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                using HttpResponseMessage response = await client.GetAsync(uri);
                string? valasz;
                if (response.IsSuccessStatusCode)
                {
                    valasz = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                    {
                        var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                        if (deserilizeResponse != null)
                        {
                            if (deserilizeResponse.IsSuccess)
                            {
                                return JsonConvert.DeserializeObject<Tv>(deserilizeResponse.Content.ToString());
                            }
                            else
                            {
                                OnMessageReceived?.Invoke(this, new MessageEvent(deserilizeResponse.ErrorMessage,100));
                                return default;
                            }
                        }
                    }
                    return JsonConvert.DeserializeObject<Tv>(valasz);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    valasz = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                    {
                        var r = JsonConvert.DeserializeObject<MainResponse>(valasz);
                        if (r?.IsSuccess ?? false)
                        {
                            ErrorMessage = r.ErrorMessage;
                        }
                        else ErrorMessage = r?.ErrorMessage ?? "Null eredmény lett a hiba üzenet";
                    }
                    else ErrorMessage = "Nem elérhető a szerver!";
                    return default;
                }
                else
                {
                    ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                    return default;
                }
            }
            catch (HttpRequestException e)
            {
                //client.CancelPendingRequests();
                //client.Dispose();
                ErrorMessage = "Nem elérhető a szerver! Hibaüzenet: " + e.Message;
                return default;
            }
            catch (Exception e)
            {
                client.CancelPendingRequests();
                //client.Dispose();
                ErrorMessage = "Hiba történt! Az üzenet: " + e.Message;
                return default;
            }
        }

        public async Task<T?> Post<T>(string uri, T t)
        {
            var valasz = string.Empty;
            //var token = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);

            if (string.IsNullOrEmpty(_token) && !uri.Contains("Math"))
            {
                await CreateAuthorizationHeaderForUserAsync();
            }
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsJsonAsync<T>(uri, t);
            if (response.IsSuccessStatusCode)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                {
                    var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    if (deserilizeResponse != null)
                    {
                        if (deserilizeResponse.IsSuccess)
                        {
                            return JsonConvert.DeserializeObject<T>(deserilizeResponse.Content?.ToString() ?? string.Empty);
                        }
                        else
                        {
                            OnMessageReceived?.Invoke(this, new MessageEvent(deserilizeResponse.ErrorMessage, 100));
                            return default;
                        }
                    }
                }
                return JsonConvert.DeserializeObject<T?>(valasz);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                {
                    var r = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    ErrorMessage = r?.ErrorMessage ?? "Null eredmény lett a hiba üzenet";
                }
                else ErrorMessage += "Nem elérhető a szerver!";
                return default;
            }
            else
            {
                ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                var message = await response.Content.ReadAsStringAsync();
                ErrorMessage += message;
            }
            return default;
        }

        public async Task<T?> Put<T>(string uri, T t)
        {
            //var token = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
            if (string.IsNullOrEmpty(_token) && !uri.Contains("Math"))
            {
                await CreateAuthorizationHeaderForUserAsync();
            }
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PutAsJsonAsync<T>(uri, t);
            string valasz;
            if (response.IsSuccessStatusCode)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                {
                    var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    if (deserilizeResponse != null)
                    {
                        if (deserilizeResponse.IsSuccess)
                        {
                            return JsonConvert.DeserializeObject<T>(deserilizeResponse.Content.ToString());
                        }
                        else
                        {
                            OnMessageReceived?.Invoke(this, new MessageEvent(deserilizeResponse.ErrorMessage, 100));
                            return default;
                        }
                    }
                }
                return JsonConvert.DeserializeObject<T>(valasz);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                {
                    var r = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    ErrorMessage = r?.ErrorMessage ?? "Null eredmény lett a hiba üzenet";
                }
                else ErrorMessage += "Nem elérhető a szerver!";
                return default;
            }
            else
            {
                ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                var message = await response.Content.ReadAsStringAsync();
                ErrorMessage += message;
            }
            return default;
        }

        public async Task<bool> Delete(string uri)
        {
            bool valasz = false;
            //var token = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
            if (string.IsNullOrEmpty(_token) && !uri.Contains("Math"))
            {
                await CreateAuthorizationHeaderForUserAsync();
            }
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.DeleteAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                valasz = true;
            }
            else ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
            return valasz;
        }
    }
    public class TokenResponse
    {
        public string? Token { get; set; }
        public int ExpiresIn { get; set; }
    }

}