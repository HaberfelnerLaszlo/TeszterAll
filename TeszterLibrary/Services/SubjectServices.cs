using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
	public class SubjectServices
	{
		private readonly TeszterApiService _service;
			public SubjectServices(TeszterApiService service)
		{
			_service = service;
		}
		public async Task<List<Subject>> GetSubjects()
		{
			return await _service.Get<List<Subject>>("Subject");
	   }
		public async Task<Subject> AddSubjectsAsync(Subject subject)
		{
			try
			{
				var v= await _service.Post("Subject",subject);
				return v;
			}
			catch (Exception)
			{
				return null;
				throw;
			}
		}
		public async Task<Subject> UpdateSubjectAsync(Subject subject)
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
		public bool Exits(string subjectName)
		{
			return true; //TODO ezt meg kell oldani!! dbContext.Subjects.Any(s => s.Name == subjectName);
		}
	}
}
