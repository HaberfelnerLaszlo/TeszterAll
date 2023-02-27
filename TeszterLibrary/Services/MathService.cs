using TeszterLibrary.Model;
using TeszterLibrary.ViewModel;

namespace TeszterLibrary.Services
{
	public class MathService
	{
		private readonly TeszterApiService _service;
		public string ErrorMessage = string.Empty;
		public MathService(TeszterApiService teszterApiService)
		{
			_service = teszterApiService;
		}
		public async Task<List<TaskList>> GetTaskList() => await _service.Get<List<TaskList>>("Math");
		public async Task<List<Lesson>> SetLessons(List<Lesson> lessons) => await _service.Post("Math", lessons);
		public async Task<List<HasLesson>> HasLessons(List<HasLesson> hasLessons) => await _service.Post("Math/has", hasLessons);
		public async Task<List<Lesson>> GetLessons(Guid id) => await _service.Get<List<Lesson>>($"Math/{id}");
		public async Task<TaskView> Start(Guid id) => await _service.Get<TaskView>($"Math/Start/{id}");
		public async Task<TaskRecive> Result(TaskRecive taskRecivie) => await _service.Post("Math/Result", taskRecivie);
		public async Task<TaskView> ClassLesson(Guid classId) => await _service.Get<TaskView>($"Math/Class/{classId}");
		public async Task<List<ClassRecive>> ClassResult(List<ClassRecive> classRecivies) => await _service.Post("Math/ClassResult", classRecivies);
		public async Task<List<Lesson>> UpdateClassLessons(List<Lesson> lessons) => await _service.Put<List<Lesson>>("Math", lessons);
	}
}
