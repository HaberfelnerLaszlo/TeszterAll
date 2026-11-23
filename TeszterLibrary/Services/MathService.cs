using TeszterLibrary.Components;
using TeszterLibrary.Model;
using TeszterLibrary.ViewModel;

namespace TeszterLibrary.Services
{
	public class MathService(TeszterApiService teszterApiService)
    {
		private readonly TeszterApiService _service = teszterApiService;
		public string ErrorMessage = string.Empty;

        public async Task<List<TaskList>> GetTaskList() => await _service.Get<List<TaskList>>("Math");
		public async Task<List<Lesson>> SetLessons(List<Lesson> lessons) => await _service.Post("Math", lessons);
		public async Task<List<ClassLesson>> SetClassLessons(List<ClassLesson> lessons) => await _service.Post("Math/Class", lessons);
		public async Task<List<HasLesson>> HasLessons(List<HasLesson> hasLessons) => await _service.Post("Math/has", hasLessons);
        public async Task<List<Lesson>> GetLessons(Guid id) => await _service.Get<List<Lesson>>($"Math/{id}");
		public async Task<TaskView> Start(Guid id) => await _service.Get<TaskView>($"Math/Start/{id}");
		public async Task<TaskRecive> Result(TaskRecive taskRecivie) => await _service.Post("Math/Result", taskRecivie);
		public async Task<TaskView> ClassLesson(Guid classId) => await _service.Get<TaskView>($"Math/Class/{classId}");
		public async Task<List<ClassLesson>> GetClassLessons(Guid classId) => await _service.Get<List<ClassLesson>>($"Math/ClassLessons/{classId}");
		public async Task<List<ClassRecive>> ClassResult(List<ClassRecive> classRecivies) => await _service.Post("Math/ClassResult", classRecivies);
		public async Task<List<Lesson>> UpdateLessons(List<Lesson> lessons) => await _service.Put("Math", lessons);
		public async Task<List<ResultClassView>> ResultClassView(string ids) => await _service.Get<List<ResultClassView>>($"Math/ResultClass/{ids}");
		public async Task<List<ResultStudentView>> ResultStudentViews(string id) => await _service.Get<List<ResultStudentView>>($"Math/ResultStudent/{id}");
		public async Task<List<ResultTaskView>> ResultTaskViews(int id) => await _service.Get<List<ResultTaskView>>($"Math/ResultTask/{id}");
		public async Task<List<TopicEditView>?> GetTopics() => await _service.Get<List<TopicEditView>>("Math/Topics");
		public async Task<TopicPreviewView?> PreviewTopic(TopicPreviewRequest request) => await _service.PostAs<TopicPreviewView>("Math/Topics/Preview", request);
		/// <summary>Téma mentése; hibánál a szerver indoklása.</summary>
		public async Task<(TopicEditView? Saved, string Error)> SaveTopic(TopicEditView topic)
		{
			var saved = await _service.PostAs<TopicEditView>("Math/Topics", topic);
			return (saved, saved is null ? _service.ErrorMessage : string.Empty);
		}
		/// <summary>Jelvény-megerősítő kihívás indítása; ha nem indítható, a szerver (barátságos) indoklása.</summary>
		public async Task<(BadgeConfirmStartView? View, string Error)> StartBadgeConfirm(string code)
		{
			var view = await _service.PostAs<BadgeConfirmStartView>($"Math/BadgeConfirm/{Uri.EscapeDataString(code)}");
			return (view, view is null ? _service.ErrorMessage : string.Empty);
		}
		public async Task<BadgeConfirmResultView?> FinishBadgeConfirm(int runId) => await _service.PostAs<BadgeConfirmResultView>($"Math/BadgeConfirm/Finish/{runId}");
		public async Task<List<Guid>?> GetGroups() => await _service.Get<List<Guid>>("Math/Groups");
		public async Task<StudentProgressView?> GetProgress(Guid id) => await _service.Get<StudentProgressView>($"Math/Progress/{id}");
		public async Task<List<LeaderboardClassView>?> GetLeaderboardClasses() => await _service.Get<List<LeaderboardClassView>>("Math/Leaderboard");
		public async Task<List<ChallengeTopicView>?> GetChallengeTopics() => await _service.Get<List<ChallengeTopicView>>("Math/Challenge");
		public async Task<ChallengeStartView?> StartChallenge(int taskId) => await _service.PostAs<ChallengeStartView>($"Math/Challenge/Start/{taskId}");
		public async Task<ChallengeAnswerView?> ChallengeAnswer(ChallengeAnswerRequest request) => await _service.PostAs<ChallengeAnswerView>("Math/Challenge/Answer", request);
		public async Task<ChallengeResultView?> FinishChallenge(int runId) => await _service.PostAs<ChallengeResultView>($"Math/Challenge/Finish/{runId}");
		public async Task<ClassChallengeView?> CreateClassChallenge(ClassChallengeRequest request) => await _service.PostAs<ClassChallengeView>("Math/ClassChallenge", request);
		public async Task StopClassChallenge(int id) => await _service.PostAs<object>($"Math/ClassChallenge/{id}/Stop");
		public async Task<List<ClassChallengeView>?> ActiveClassChallenges() => await _service.Get<List<ClassChallengeView>>("Math/ClassChallenge/Active");
		public async Task<ChallengeStartView?> JoinClassChallenge(int id) => await _service.PostAs<ChallengeStartView>($"Math/ClassChallenge/{id}/Join");
		public async Task<ClassChallengeView?> ClassChallengeStandings(int id) => await _service.Get<ClassChallengeView>($"Math/ClassChallenge/{id}");
		public async Task<LeaderboardView?> GetLeaderboard(string classId) => await _service.Get<LeaderboardView>($"Math/Leaderboard/{Uri.EscapeDataString(classId)}");
    }
}
