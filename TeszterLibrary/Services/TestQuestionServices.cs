using Microsoft.Extensions.Logging;

using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
	public class TestQuestionServices
	{
		#region Private members
		private readonly TeszterApiService _service;
		private readonly ILogger<TestQuestionServices> _logger;
        public string ErrorMessage = string.Empty;

        #endregion
        #region Constructor
        public TestQuestionServices(TeszterApiService service, ILogger<TestQuestionServices> logger)
        {
            _service = service;
            _logger = logger;
        }
        #endregion

        /// <summary>
        /// Egy adott tesztkérdés lekérdezése.
        /// </summary>
        /// <param name="id">TestQuestionId</param>
        /// <returns></returns>
        #region Public methods
        public async Task<TestQuestion> GetTestQuestion(int id)
		{
			var tq = await _service.Get<TestQuestion>($"Tq/{id}");
			return tq;
			//return dbContext.TestQuestions.SingleOrDefault(t => t.Id == id);
		}
		/// <summary>
		/// Egy adott teszt kérdéseinek lekérdezése
		/// </summary>
		/// <param name="testId">TestId</param>
		/// <returns></returns>
		public async Task<List<TestQuestion>?> GetTestQuestions(int testId)
		{
			var list = await _service.Get<List<TestQuestion>>($"Tq/Test/{testId}");
			if (list is null)
			{
				ErrorMessage = _service.ErrorMessage;
				return null;
			}
			return list;
		}
		public async Task<TestQuestion> AddTestQuestionAsync(TestQuestion testQ)
		{
			try
			{
				var tq = await _service.Post<TestQuestion>("Tq",testQ);
				return tq;
			}
			catch (Exception)
			{
				return null;//TODO Log-olni a hibát!
			}
		}
		public async Task<bool> DeleteTestQuestionAsync(int id)
		{
			try
			{
				var t = await _service.Delete($"Tq/{id}");
				return t;
			}
			catch (Exception)
			{
				throw;//TODO Log-olni kell!
			}
		}
		public async Task<bool> DeleteTestQuestionAsync(Guid Qid, int tId)
		{
			try
			{
				var t = await _service.Delete($"Tq/{Qid}/{tId}");
				return t;
			}
			catch (Exception)
			{
				throw;//TODO Log-olni kell!
			}
		}
		public async Task UpdateTestAsync(TestQuestion testQ)
		{
			try
			{
				var tq = await _service.Put<TestQuestion>($"TQ/{testQ.Id}",testQ);
			}
			catch (Exception)
			{
				throw;//TODO Log-olni kell!
			}
		}
		public async Task<bool> ExitsQuestion(int TestId, Guid QuestionId)
		{
			var TestQ = await _service.Get<bool>($"Tq/{QuestionId}/{TestId}");
			return TestQ;
		}
		#endregion
	}

}

