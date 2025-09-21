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

		public async Task<bool> UserExits(Guid id)
		{
			var list = await _service.Get<List<EduUser>>("User");
			if (list == null)
			{
				ErrorMessage = _service.ErrorMessage;
				return false;
			}
			return list?.Any(t => t.Id == id) ?? false;
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