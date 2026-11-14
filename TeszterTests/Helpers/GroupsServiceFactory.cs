using Microsoft.EntityFrameworkCore;

using TeszterLibrary.Model;

using TeszterWeb.Data;
using TeszterWeb.Services;

namespace TeszterTests.Helpers
{
    /// <summary>Valódi GroupsService memóriabeli (InMemory) adatbázissal, mint az alkalmazásban.</summary>
    public static class GroupsServiceFactory
    {
        public static GroupsService Create(params Group[] groups)
        {
            var options = new DbContextOptionsBuilder<TeszterLocalDB>()
                .UseInMemoryDatabase("teszter-tests-" + Guid.NewGuid().ToString("N"))
                .Options;
            var db = new TeszterLocalDB(options);
            db.Groups.AddRange(groups);
            db.SaveChanges();
            return new GroupsService(db);
        }
    }
}
