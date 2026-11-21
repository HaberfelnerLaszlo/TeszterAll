using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
	public class EduUserServices
	{
		private readonly TeszterApiService _service;
		public string ErrorMessage = string.Empty;

		public EduUserServices(TeszterApiService service)
		{
			_service = service;
		}
		public async Task<List<EduUser>> GetAllEduUsers()
		{
			var list = await _service.Get<List<EduUser>>("User");
			if (list == null)
			{
				ErrorMessage = _service.ErrorMessage;
				return new List<EduUser>();
			}
			return list;
        }
        public async Task<EduUser?> GetEduUser(Guid id)
		{
			return await _service.Get<EduUser>($"User/{id}");
		}

		/// <summary>
		/// Létezik-e a felhasználó. Az adott rekordot kéri le (GET User/{id}: saját maga vagy tanár), nem a teljes
		/// felhasználólistát, mint régen (az tanári végpont lett, és diáknál minden felhasználót letöltött).
		/// A 404 "nem létezik" (ErrorMessage üres, így a hívó felveheti); más hiba esetén ErrorMessage-et állít.
		/// </summary>
		public async Task<bool> UserExits(Guid id)
		{
			ErrorMessage = string.Empty;
			var user = await _service.Get<EduUser>($"User/{id}");
			if (user is not null) return true;
			if (_service.LastStatusCode != System.Net.HttpStatusCode.NotFound)
			{
				ErrorMessage = _service.ErrorMessage;
			}
			return false;
		}

		public async Task<EduUser?> AddUser(EduUser user)
		{
			try
			{
				var v = await _service.Post("User", user);
				return v;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				return null;
			}
		}

		public async Task<EduUser?> UpdateUser(EduUser user)
		{
			try
			{
				var u = await _service.Put<EduUser>($"User/{user.Id}", user);
				return u;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				return null;
			}
		}

		public async Task DeleteUser(EduUser user)
		{
			try
			{
				var v = await _service.Delete($"User/{user.Id}");
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
			}
		}
	}
}