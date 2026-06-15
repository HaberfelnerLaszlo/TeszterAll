using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
	public class QuestionServices
	{
		#region Private members
		private readonly TeszterApiService _service;
		public string Error = string.Empty;
		#endregion
		#region Constructor
		public QuestionServices(TeszterApiService service)
		{
			_service = service;
		}
		#endregion
		#region Public methods
		public async Task<Question> GetQuestion(Guid id)
		{
			var q = await _service.Get<Question>($"Question/{id}");
			return q;
		}
		/// <summary>
		/// This method returns the list of product
		/// </summary>
		/// <returns></returns>
		public async Task<List<Question>?> GetQuestions()
		{
			var list = await _service.Get<List<Question>>("Question");
			if (list == null) { Error = _service.ErrorMessage; return default; }
			return list;
		}
		public async Task<List<Question>?> GetQuestions(Guid UserId)
		{
			var list = await _service.Get<List<Question>>("Question");
			if (list == null) { Error = _service.ErrorMessage; return default; }
			return list.Where(q => q.CreatedBy == UserId).ToList();
		}
		public async Task<List<Question>> SearchQuestion(string search)
		{
			var list = await _service.Get<List<Question>>("Question");
			return list.Where(q => q.Title.Contains(search) || q.Text.Contains(search)).ToList();
		}
		/// <summary>
		/// This method add a new product to the DbContext and saves it
		/// </summary>
		/// <param name="product"></param>
		/// <returns></returns>
		public async Task<Question> AddQuestionAsync(Question question)
		{
			try
			{
				var v = await _service.Post("Question",question);
				if (v is null)
				{
					Error = _service.ErrorMessage;
					return default;
				}
			}
			catch (Exception)
			{
				throw;
			}
			return question;
		}
		/// <summary>
		/// This method update and existing product and saves the changes
		/// </summary>
		/// <param name="question"></param>
		/// <returns></returns>
		public async Task<Question?> UpdateQuestionAsync(Question question)
		{
			try
			{
				var q =await _service.Put($"Question/{question.Id}",question);
				return q;
			}
			catch (Exception)
			{
				return null;
			}
		}
		/// <summary>
		/// This method removes and existing product from the DbContext and saves it
		/// </summary>
		/// <param name="question"></param>
		/// <returns></returns>
		public async Task DeleteQuestionAsync(Question question)
		{
			try
			{
				var v = await _service.Delete($"Question/{question.Id}");
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion
	}
}
