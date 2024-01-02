using Newtonsoft.Json;

using System.Net.Http.Headers;
using System.Net.Http.Json;

using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
    public class TeszterApiService
    {
        public string ErrorMessage { get; set; } = String.Empty;
        private static readonly HttpClient client = new();

        protected static string URI = "https://teszter-api.haberfelner.eu/";

        //protected static string URI = "http://localhost:7025/";
        //protected static string URI = "https://localhost:44308/"; //ISS
        //protected static string URI = "https://z8m54ndx-7026.euw.devtunnels.ms"; //távoli
        //protected static string URI = "http://192.168.1.69:7025"; //laptop
        public async Task<Tv?> Get<Tv>(string uri)
        {
            var valasz = string.Empty;
            try
            {
                if (client.BaseAddress is null)
                {
                    client.BaseAddress = new Uri(URI);
                }
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    valasz = await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = "Nem elérhető a szerver!";
                    return default;
                }
                else
                {
                    ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                    return default;
                }
                return JsonConvert.DeserializeObject<Tv>(valasz);
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
                client.Dispose();
                ErrorMessage = "Hiba történt! Az üzenet: " + e.Message;
                return default;
            }
        }

        public async Task<T> Post<T>(string uri, T t)
        {
            var valasz = string.Empty;
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.PostAsJsonAsync<T>(uri, t);
            if (response.IsSuccessStatusCode)
            {
                valasz = await response.Content.ReadAsStringAsync();
                var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                if (deserilizeResponse != null)
                {
                    if (deserilizeResponse.IsSucccess)
                    {
                        return JsonConvert.DeserializeObject<T>(deserilizeResponse.Content.ToString());
                    }
                }
            }
            else
            {
                ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                var message = await response.Content.ReadAsStringAsync();
                ErrorMessage += message;
            }
            return JsonConvert.DeserializeObject<T>(valasz);
        }

        public async Task<T> Put<T>(string uri, T t)
        {
            string valasz = String.Empty;
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.PutAsJsonAsync<T>(uri, t);
            if (response.IsSuccessStatusCode)
            {
                valasz = await response.Content.ReadAsStringAsync();
            }
            else
            {
                ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                var message = await response.Content.ReadAsStringAsync();
                ErrorMessage += message;
            }
            return JsonConvert.DeserializeObject<T>(valasz);
        }

        public async Task<bool> Delete(string uri)
        {
            bool valasz = false;
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.DeleteAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                valasz = true;
            }
            else ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
            return valasz;
        }
    }
}