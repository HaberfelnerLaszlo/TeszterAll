using Microsoft.Extensions.Logging;

using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
	public class SubjectServices
	{
		private readonly TeszterApiService _service;
		private readonly ILogger<SubjectServices> _logger;
		public string ErrorMessage = string.Empty;
        public SubjectServices(TeszterApiService service, ILogger<SubjectServices> logger)
        {
            _service = service;
            _logger = logger;
        }
        public async Task<List<Subject?>?> GetSubjects()
		{
			var response=await _service.Get<List<Subject?>?>("Subject");
			if (response == null) { ErrorMessage=_service.ErrorMessage; return null; }
			return response;
		}
		public async Task<Subject?> AddSubjectsAsync(Subject subject)
		{
			try
			{
				var v= await _service.Post("Subject",subject);
				if (v is null)
				{
					ErrorMessage = _service.ErrorMessage;
					_logger.LogWarning(ErrorMessage);
					return null;
				}
				return v;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				_logger.LogError(ErrorMessage,e);
				return null;
			}
		}
		public async Task<Subject?> UpdateSubjectAsync(Subject subject)
		{
			try
			{
				var s = await _service.Put($"Subject/{subject.Id}", subject);
				return s;
			}
			catch (Exception)
			{
				return null;
			}
		}
		public async Task DeleteSubjectAsync(Subject subject)
		{
			try
			{
				var v = await _service.Delete($"Subject/{subject.Id}");
			}
			catch (Exception)
			{
				throw;
			}
		}
		public async Task<bool> Exits(string subjectName)
		{
			var response=await _service.Get<bool>(uri: $"Subject/Exits/{subjectName}");
			return response;
		}
	}
}
