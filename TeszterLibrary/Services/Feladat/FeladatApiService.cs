using Newtonsoft.Json;

using System.Net.Http.Headers;
using System.Net.Http.Json;

using TeszterLibrary.Model;

namespace TeszterLibrary.Services.Feladat
{
    public class FeladatApiService
    {
        public string ErrorMessage { get; set; } = string.Empty;
        private static readonly HttpClient client = new();
        protected static string URI = "https://fapi.haberfelner.eu/";
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
                    if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                    {
                    var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    if (deserilizeResponse != null)
                    {
                        if (deserilizeResponse.IsSuccess)
                        {
                            return JsonConvert.DeserializeObject<Tv>(deserilizeResponse.Content.ToString());
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
                        if (r?.IsSuccess??false)
                        {
                            ErrorMessage = r.ErrorMessage;
                        }
                        else ErrorMessage = r?.ErrorMessage??"Null eredmény lett a hiba üzenet";
                    }else ErrorMessage = "Nem elérhető a szerver!";
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
                ErrorMessage = "Nem elérhető a szerver! Hibaüzenet: " + e.Message;
                return default;
            }
            catch (Exception e)
            {
                client.CancelPendingRequests();
                ErrorMessage = "Hiba történt! Az üzenet: " + e.Message;
                return default;
            }
        }
        public async Task<T?> Post<T>(string uri, T t)
        {
            var valasz = string.Empty;
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.PostAsJsonAsync(uri, t);
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

        public async Task<bool> Put<T>(string uri, T t)
        {
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.PutAsJsonAsync(uri, t);
            string valasz;
            if (response.IsSuccessStatusCode)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                {
                    var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    if (deserilizeResponse != null)
                    {
                        return deserilizeResponse.IsSuccess;
                        //if (deserilizeResponse.IsSuccess)
                        //{
                            //return JsonConvert.DeserializeObject<T>(deserilizeResponse.Content.ToString());
                        //}
                    }
                }
                return false;//JsonConvert.DeserializeObject<T>(valasz);
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
                return false;
            }
            else
            {
                ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                var message = await response.Content.ReadAsStringAsync();
                ErrorMessage += message;
            }
            return false;
        }
        public async Task<bool> Delete(string uri)
        {
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.DeleteAsync(uri);
            string valasz;
            if (response.IsSuccessStatusCode)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("errorMessage"))
                {
                    var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    if (deserilizeResponse != null)
                    {
                        return deserilizeResponse.IsSuccess;
                        //if (deserilizeResponse.IsSuccess)
                        //{
                        //return JsonConvert.DeserializeObject<T>(deserilizeResponse.Content.ToString());
                        //}
                    }
                }
                return false;//JsonConvert.DeserializeObject<T>(valasz);
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
                return false;
            }
            else
            {
                ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                var message = await response.Content.ReadAsStringAsync();
                ErrorMessage += message;
            }
            return false;
        }
    }
}
