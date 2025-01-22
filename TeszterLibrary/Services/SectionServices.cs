using Microsoft.Extensions.Logging;

using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
	public class SectionServices(TeszterApiService service, ILogger<SectionServices> logger)
    {
        public string ErrorMessage = string.Empty;
        public async Task<List<Section>?> GetSections()
		{
			var response=await service.Get<List<Section>>("Section");
			if (response == null) 
			{
				ErrorMessage=service.ErrorMessage;
				logger.LogWarning("A szekciók lekérdezésekor hiba lépet fel: {ErrorMessage}",ErrorMessage);
				return null;
			}
			return response;
		}
        public async Task<List<Section>?> GetSections(int testId)
		{
			var response=await service.Get<List<Section>?>($"Section/{testId}");
			if (response == null) 
			{
				ErrorMessage=service.ErrorMessage;
				logger.LogWarning("A szekciók({testId}) lekérdezésekor hiba lépet fel: {ErrorMessage}",testId,ErrorMessage);
				return null;
			}
			return response;
		}
		public async Task<Section?> AddSectionsAsync(Section Section)
		{
			try
			{
				var v= await service.Post("Section",Section);
				if (v is null)
				{
					ErrorMessage = service.ErrorMessage;
					logger.LogWarning(ErrorMessage);
					return null;
				}
				return v;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				logger.LogError("A rekord felvételénél hiba történt: {ErrorMessage} Teljes: {e}",ErrorMessage,e);
				return null;
			}
		}
		public async Task<Section?> UpdateSectionAsync(Section Section)
		{
			try
			{
				var s = await service.Put<Section>($"Section/{Section.Id}", Section);
				return s;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				logger.LogError("A frissítésnél hiba történt: {ErrorMessage} Teljes: {e}",ErrorMessage,e);
				return null;
			}
		}
		public async Task DeleteSectionAsync(Section Section)
		{
			try
			{
				var v = await service.Delete($"Section/{Section.Id}");
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				logger.LogError("A törlésnél hiba történt: {ErrorMessage}",ErrorMessage);
			}
		}
		//public bool Exits(string SectionName)
		//{
		//	return true; //TODO ezt meg kell oldani!! dbContext.Sections.Any(s => s.Name == SectionName);
		//}
	}
}
