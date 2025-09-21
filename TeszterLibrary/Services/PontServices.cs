using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeszterLibrary.Services.Feladat;
using TeszterLibrary.ViewModel;

namespace TeszterLibrary.Services
{
    public class PontServices(FeladatApiService apiService)
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public async Task<TanuloData?> GetTanuloData(string id)
        {
            var tanuloData =await apiService.Get<TanuloData>($"tanulo_data/{id}");
            if (tanuloData is null)
            {
                ErrorMessage = apiService.ErrorMessage;
                return null;
            }
            return tanuloData;
        }
        public async Task<List<Pont>?> GetTanulokPontokByOsztaly(string osztaly)
        {
            var tanuloPontok = await apiService.Get<List<Pont>>($"pont/osztaly/{osztaly}");
            if (tanuloPontok is null)
            {
                ErrorMessage = apiService.ErrorMessage;
                return null;
            }
            return tanuloPontok;
        }
    }
}
