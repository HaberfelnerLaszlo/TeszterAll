using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeszterLibrary.Model;
using TeszterLibrary.ViewModel;

namespace TeszterLibrary.Services
{
	public class TestUserServices
	{
		private readonly TeszterApiService _service;
		public string ErrorMessage=string.Empty;
		public TestUserServices(TeszterApiService service)
		{
			_service = service;
		}
		public async Task<TestUser?> GetTestUser(int id)
		{
			var tu = await _service.Get<TestUser?>($"TUser/{id}");
			return tu;
		}
		/// <summary>
		/// Visszaadd egy testUser objectumot egy teszt, egy felhasználó alapján
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <param name="testId">Teszt azonosító</param>
		/// <returns></returns>
		public async Task<TestUser?> GetTestUser(Guid userId, int testId)
		{
			var tu = await _service.Get<TestUser?>($"TUser/TU/{userId}/{testId}");
			return tu;
		}
		/// <summary>
		/// Visszaadja egy adott teszt résztvevőinek tesztUser adatait.
		/// </summary>
		/// <param name="tId">Teszt azonosítója</param>
		/// <returns></returns>
		public async Task<List<TestUser>?> GetTestUsers(int tId)
		{
			var list = await _service.Get<List<TestUser>?>($"TUser/Member/{tId}");
			return list;
		}
		/// <summary>
		/// Megaddja egy felhasználó összes még nem kész tesztjét.
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <returns></returns>
		public async Task<List<TestView>?> GetTests(Guid userId)
		{
			var tests = await _service.Get<List<TestView>?>($"TUser/Tests/{userId}");
			if ( tests == null ) { ErrorMessage = _service.ErrorMessage; return null; }
			return tests;
		}
		public async Task<List<QuestionView>?> GetTestQuestions(int tUserId)
		{
			var response = await _service.Get<List<QuestionView>?>($"TUser/Test/{tUserId}");
			if (response is null)
			{
				ErrorMessage = _service.ErrorMessage; 
				return null;
			}
			return response;
		}
		/// <summary>
		/// A tesztet írok száma.
		/// </summary>
		/// <param name="testId">Teszt azonosítója</param>
		/// <returns></returns>
		public async Task<int[]?> GetActiveTestMemberCount(int testId)
		{
			var m = await _service.Get<int[]?>($"Tuser/Member/{testId}");
			return m;
		}
		/// <summary>
		/// Kész/lezárt teszteket adja eredményül felhasználónként
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <returns></returns>
		public async Task<List<TestUser>?> GetReadyTests(Guid userId)
		{
			var list = await _service.Get<List<TestUser>?>($"TUser/Ready/{userId}");
			return list;
		}
		/// <summary>
		/// Egy adott felhasznló összes tesztje.
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <returns></returns>
		public async Task<List<TestUser>?> GetAllUserTests(Guid userId)
		{
			var list = await _service.Get<List<TestUser>?>($"TUser/All/{userId}");
			return list;
		}
		public async Task<QuestionView?> GetQuestionResult(QuestionView questionView,int tUserId)
		{
			var response = await _service.Post<QuestionView>($"TUser/Result/{tUserId}",questionView);
			if (response is null) return null;
			return response;
		}
		public async Task<QuestionView?> GetQuestionCheck(QuestionView questionView,int tUserId)
		{
			var response = await _service.Post<QuestionView>($"TUser/Check/{tUserId}",questionView);
			if (response is null) return null;
			return response;
		}
		public async Task<TestUser?> AddTestUser(TestUser testUser)
		{
			try
			{
				var u = await _service.Post<TestUser?>("TUser",testUser);
				return u;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				return null;
			}
		}
		public async Task<TestUser?> UpdateTestUser(TestUser testUser)
		{
			try
			{
				var u = await _service.Put<TestUser?>($"TUser/{testUser.Id}", testUser);
				return u;
			}
			catch (Exception e)
			{
				ErrorMessage=e.Message;
				return null;
			}
		}
	}
}
