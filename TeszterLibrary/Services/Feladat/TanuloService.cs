using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeszterLibrary.Model.Feladat;

namespace TeszterLibrary.Services.Feladat
{
    public class TanuloService(FeladatApiService apiService,ILogger<TanuloService> logger)
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public async Task<List<Tanulo>?> GetTanulokAsync()
        {
            var response = await apiService.Get<List<Tanulo>>("tanulo");
            if (response == null)
            {
                ErrorMessage = apiService.ErrorMessage;
                logger.LogWarning("A tanulók lekérdezésekor hiba lépet fel: {ErrorMessage}", ErrorMessage);
                return null;
            }
            return response;
        }
        public async Task<Tanulo?> GetTanuloAsync(int id)
        {
            var response = await apiService.Get<Tanulo>($"tanulo/{id}");
            if (response == null)
            {
                ErrorMessage = apiService.ErrorMessage;
                logger.LogWarning("A tanuló({id}) lekérdezésekor hiba lépet fel: {ErrorMessage}", id, ErrorMessage);
                return null;
            }
            return response;
        }
        public async Task<Tanulo?> AddTanuloAsync(Tanulo tanulo)
        {
            try
            {
                var v = await apiService.Post("tanulo", tanulo);
                if (v is null)
                {
                    ErrorMessage = apiService.ErrorMessage;
                    logger.LogWarning(ErrorMessage);
                    return null;
                }
                return v;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                logger.LogError("A rekord felvételénél hiba történt: {ErrorMessage} Teljes: {e}", ErrorMessage, e);
                return null;
            }
        }
        public async Task<bool> UpdateTanuloAsync(Tanulo tanulo)
        {
            try
            {
                var s = await apiService.Put<Tanulo>($"tanulo/{tanulo.Id}", tanulo);
                return s;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                logger.LogError("A rekord frissítésénél hiba történt: {ErrorMessage} Teljes: {e}", ErrorMessage, e);
                return false;
            }
        }
        public async Task<bool> DeleteTanuloAsync(Guid id)
        {
            try
            {
                var s = await apiService.Delete($"tanulo/{id}");
                return s;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                logger.LogError("A rekord törlésénél hiba történt: {ErrorMessage} Teljes: {e}", ErrorMessage, e);
                return false;
            }
        }
    }
}
