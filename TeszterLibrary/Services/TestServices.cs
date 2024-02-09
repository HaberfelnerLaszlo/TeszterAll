
using TeszterLibrary.Model;
using TeszterLibrary.ViewModel.Test;

namespace TeszterLibrary.Services
{
    public class TestServices
    {
        #region Private members
        private readonly TeszterApiService _service;
        public string ErrorMessage=string.Empty;
        #endregion
        #region Constructor
        public TestServices(TeszterApiService service)
        {
            _service = service;
        }
        #endregion
        #region Public methods
        public async Task<Test> GetTest(int id)
        {
            var t= await _service.Get<Test>($"Test/{id}");
            return t;
        }
        public async Task<List<Test>> GetTests()
        {
            var t= await _service.Get<List<Test>>("Test");
            return t;
        }
        /// <summary>
        /// Egy publikus teszt visszaadása
        /// </summary>
        /// <param name="id">teszt azonosító</param>
        /// <returns></returns>
        public async Task<Test> GetPublishedTest(int id)
        {
            var test = await _service.Get<Test>($"Test/Publish/{id}");
            if (test != null)
            {
                if (test.IsPublished)
                {
                    return test;
                }
                else return null;
            }
            else return null;
        }
        /// <summary>
        /// Nem aktív/ütemezett tesztek a tesztkészítő számára.
        /// </summary>
        /// <param name="uId">Tesztkészítő azonosító</param>
        /// <returns></returns>
        public async Task<List<Test>> GetNoActiveTest(Guid uId)
        {
            var list = await _service.Get<List<Test>>($"Test/NoActive/{uId}");
            return list;
        }
        /// <summary>
        /// Nem kész tesztek a tesztkészítő számára.
        /// </summary>
        /// <param name="uId">Tesztkészítő azonosító</param>
        /// <returns></returns>
        public async Task<List<Test>?> GetNoPublished(Guid uId)
        {
            var list = await  _service.Get < List<Test>>($"Test/NoPublish/{uId}");
            if (list is null)
            {
                ErrorMessage = _service.ErrorMessage;
                return null;
            }
            return list;
        }
        /// <summary>
        /// Aktív tesztek a tesztkészítő számára.
        /// </summary>
        /// <param name="uId">Tesztkészítő azonosítója</param>
        /// <returns></returns>
        public async Task<List<ActiveTestView>?> GetActiveTests(Guid uId)
        {
            var list = await _service.Get<List<ActiveTestView>>($"Test/Active/{uId}");
            if ( list is null ||!list.Any())
            {
                ErrorMessage=_service.ErrorMessage; 
                return null;
            }
            return list;
        }
        /// <summary>
        /// Lezárt tesztek a tesztkészítő számára.
        /// </summary>
        /// <param name="uId">Tesztkészítő azonosító</param>
        /// <returns></returns>
        public async Task<List<TestResultToTeacher>?> GetClosedTests(Guid uId)
        {
            var list = await _service.Get<List<TestResultToTeacher>>($"Test/Closed/{uId}");
            if ( list is null ||!list.Any())
            {
                ErrorMessage=_service.ErrorMessage; 
                return null;
            }
            return list;
        }
        /// <summary>
        /// Teszt mentése adatbázisba
        /// </summary>
        /// <param name="test">Teszt egy példánya</param>
        /// <returns></returns>
        public async Task<Test> AddTest(Test test)
        {
            try
            {
                var t = await _service.Post<Test>("Test",test);
                return t;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return null;
            }
        }
        public async Task<bool> DeleteTestAsync(int id)
        {
            var response=await _service.Delete($"Test/{id}"); //TODO Sikertelenség esetén teendők
            return response;
        }
        public async Task<Test> UpdateTestAsync(Test test)
        {
            try
            {
                var t = await _service.Put<Test>($"Test/{test.Id}",test);
                return t;
            }
            catch (Exception e)
            {
                ErrorMessage=e.Message;
                return null; //TODO Modósítási kivételek kezelésének megírása.
            }
        }
        #endregion
    }
}
