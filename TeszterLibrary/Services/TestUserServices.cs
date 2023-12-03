using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeszterLibrary.Model;

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
		public async Task<TestUser> GetTestUser(int id)
		{
			var tu = await _service.Get<TestUser>($"TUser/{id}");
			return tu;
		}
		/// <summary>
		/// Visszaadd egy testUser objectumot egy teszt, egy felhasználó alapján
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <param name="testId">Teszt azonosító</param>
		/// <returns></returns>
		public async Task<TestUser> GetTestUser(Guid userId, int testId)
		{
			var tu = await _service.Get<TestUser>($"TUser/TU/{userId}/{testId}");
			return tu;
		}
		/// <summary>
		/// Visszaadja egy adott teszt résztvevőinek tesztUser adatait.
		/// </summary>
		/// <param name="tId">Teszt azonosítója</param>
		/// <returns></returns>
		public async Task<List<TestUser>> GetTestUsers(int tId)
		{
			var list = await _service.Get<List<TestUser>>($"TUser/Member/{tId}");
			return list;
		}
		/// <summary>
		/// Megaddja egy felhasználó összes még nem kész tesztjét.
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <returns></returns>
		public async Task<List<TestUser>> GetTests(Guid userId)
		{
			var tests = await _service.Get<List<TestUser>>($"TUser/Tests/{userId}");
			return tests;
		}
		//public List<ApiTest> GetActiveTests(Guid userId)
		//{
		//	List<ApiTest> viewTests = new List<ApiTest>();
		//	var Utests = dbContext.TestUsers.Where(tu => tu.UserId == userId && !tu.IsReady).ToList();

		//	//Itt figyeli hogy amelyik teszt lejárt az már kész tesztként legyen nyilvántartva.
		//	List<TestUser> RemovebleTest = new();
		//	foreach (var Utest in Utests)
		//	{
		//		var test = dbContext.Tests.SingleOrDefault(t => t.Id == Utest.TestId);
		//		if (test.EndTime < DateTime.Now)
		//		{
		//			Utest.IsReady = true;
		//			RemovebleTest.Add(Utest);
		//			dbContext.TestUsers.Update(Utest);
		//		}
		//		else
		//		{
		//			var tq = dbContext.TestQuestions.Where(tq => tq.TestId == test.Id).ToList();
		//			List<ApiTestQuestion> apiQuestion = new List<ApiTestQuestion>();
		//			foreach (var item in tq)
		//			{
		//				apiQuestion.Add(new ApiTestQuestion()
		//				{
		//					QuestionId = item.QuestionId,
		//					Score = item.Score,
		//					TestQuestionId = item.Id,
		//					Time = item.Time
		//				});
		//			}
		//			viewTests.Add(
		//				new ApiTest()
		//				{
		//					TestId = test.Id,
		//					TestUserId = Utest.Id,
		//					ReplaceNumber = test.ReplaceNumber,
		//					ReplaceNumberUser = Utest.ReplaceNumber,
		//					EndTime = test.EndTime,
		//					IsPractice = test.IsPractice,
		//					IsTimer = test.IsTimer,
		//					ShuffleQuestion = test.ShuffleQuestion,
		//					StartTime = test.StartTime,
		//					Summa = test.Summa,
		//					Title = test.Title,
		//					Time = test.Time,
		//					Subject = test.Subject,
		//					TestQuestions = apiQuestion
		//				});
		//		}
		//	}
		//	foreach (var item in RemovebleTest)
		//	{
		//		Utests.Remove(item);
		//	}
		//	return viewTests;
		//}

		/// <summary>
		/// A tesztet írok száma.
		/// </summary>
		/// <param name="testId">Teszt azonosítója</param>
		/// <returns></returns>
		public async Task<int[]> GetActiveTestMemberCount(int testId)
		{
			var m = await _service.Get<int[]>($"Tuser/Member/{testId}");
			return m;
		}
		/// <summary>
		/// A próbálkozások számát adja visza adott teszt és felhasználó esetén.
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <param name="testId">Teszt azonosító</param>
		/// <returns></returns>
		//public int DifferenceReplaceNumber(Guid userId, int testId)
		//{
		//	var testRN = dbContext.Tests.SingleOrDefault(t=>t.Id == testId).ReplaceNumber;
		//	var testURN =dbContext.TestUsers.SingleOrDefault(tu => tu.UserId == userId && tu.TestId == testId).ReplaceNumber;
		//	return testRN - testURN;
		//}
		/// <summary>
		/// Kész/lezárt teszteket adja eredményül felhasználónként
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <returns></returns>
		public async Task<List<TestUser>> GetReadyTests(Guid userId)
		{
			var list = await _service.Get<List<TestUser>>($"TUser/Ready/{userId}");
			return list;
		}
		/// <summary>
		/// Egy adott felhasznló összes tesztje.
		/// </summary>
		/// <param name="userId">Felhasználó azonosító</param>
		/// <returns></returns>
		public async Task<List<TestUser>> GetAllUserTests(Guid userId)
		{
			var list = await _service.Get<List<TestUser>>($"TUser/All/{userId}");
			return list;
		}
		public async Task<TestUser> AddTestUser(TestUser testUser)
		{
			try
			{
				var u = await _service.Post<TestUser>("TUser",testUser);
				return u;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				return null;
			}
		}
		public async Task<TestUser> UpdateTestUser(TestUser testUser)
		{
			try
			{
				var u = await _service.Put<TestUser>($"TUser/{testUser.Id}", testUser);
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
