using Microsoft.Extensions.Logging;

using TeszterLibrary.Model;
using TeszterLibrary.ViewModel.Test;

namespace TeszterLibrary.Services
{
	public class TestQuestionUserServices
	{
		private readonly ILogger<TestQuestionUserServices> _logger;
		private readonly TeszterApiService _service;
		public string errorMessage=string.Empty;
        public TestQuestionUserServices(TeszterApiService service, ILogger<TestQuestionUserServices> logger)
        {
            _service = service;
            _logger = logger;
        }
		/// <summary>
		/// Azonosító alapján egy felhasználóhoz köthető kérdés adatait adja vissza.
		/// </summary>
		/// <param name="id">Azonosító</param>
		/// <returns></returns>
		public async Task<TestQuestionUser?> GetQuestionUser(int id)
		{
			var tqu = await _service.Get<TestQuestionUser?>($"Tqu/{id}");
			if ( tqu == null ) 
			{
				errorMessage=_service.ErrorMessage;
				_logger.LogWarning( errorMessage );
				return null; }
			return tqu;
		}
		/// <summary>
		/// Egy adott teszt, adott felhasználójához köthető adatokat adja vissza
		/// </summary>
		/// <param name="testUserId"></param>
		/// <returns>Lista</returns>
		public async Task<List<TestQuestionUser>> GetTestQuestionUsers(int testUserId)
		{
			var list = await _service.Get<List<TestQuestionUser>>($"Tqu/All/{testUserId}");
			return list;
		}
		//public async Task<List<TestQuestionUser>> GetTestQuestionUsers(int testUserId, int rNumber)
		//{
		//	var list = await _service.Get<List<TestQuestionUser>>($"Tqu/{testUserId}/{rNumber}");
		//	return list;
		//}
		/// <summary>
		/// Az eredeti kérdés a válaszokkal.
		/// </summary>
		/// <param name="testId">TestId azonosító</param>
		/// <returns>ViewQuestion</returns>
		public async Task<List<UserResultToTeacher>?> UsersResult(int testId)
		{
			var list = await _service.Get<List<UserResultToTeacher>>($"Tqu/Result/{testId}");
			if (list == null)
			{
				errorMessage=_service.ErrorMessage;
				_logger.LogError(errorMessage);
				return null;
			}
			return list;
		}
		//public async Task<TestQuestionUser> AddTestQuestionUser(TestQuestionUser testQuestionUser)
		//{
		//	try
		//	{
		//		foreach (var item in testQuestionUser.Answers)
		//		{
		//			item.TestQuestionUserId = 0;
		//			item.Id = 0;
		//		}
		//		var tqu = await _service.Post<TestQuestionUser>("Tqu", testQuestionUser);
		//		if (tqu is not null)
		//		{
		//			return tqu;
		//		}
		//		return null;
		//	}
		//	catch (System.Exception)
		//	{
		//		return null;
		//		throw;
		//	}
		//}
		public async Task<TestQuestionUser?> UpdateTestQuestionUser(TestQuestionUser testQuestionUser)
		{
			try
			{
				var tqu = await _service.Put<TestQuestionUser?>($"Tqu/{testQuestionUser.Id}", testQuestionUser);
				if (tqu is not null)
				{
					return tqu;
				}
				errorMessage=_service.ErrorMessage;
				_logger.LogWarning( errorMessage );
				return null;
			}
			catch (System.Exception e)
			{
				errorMessage=e.Message;
				_logger.LogError(errorMessage,e);
				return null;
			}
		}
		public async Task<bool> UpdateScore(int Id,int Score)
		{
			try
			{
				var tqu = await _service.Put<int>($"Tqu/Score/{Id}", Score);
				if (tqu>0)
				{
					return true;
				}
				errorMessage=_service.ErrorMessage;
				_logger.LogWarning( errorMessage );
				return false;
			}
			catch (System.Exception e)
			{
				errorMessage=e.Message;
				_logger.LogError(errorMessage);
				return false;
			}
		}
	}
}
