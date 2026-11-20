using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using TeszterLibrary.Model;

using TeszterWeb.Data;
using TeszterWeb.Services;

namespace TeszterTests.Helpers
{
    /// <summary>Valódi GroupsService memóriabeli (InMemory) adatbázissal és DbContext-gyártóval, mint az alkalmazásban.</summary>
    public static class GroupsServiceFactory
    {
        public static GroupsService Create(params Group[] groups) => new(CreateDbFactory(groups));

        /// <summary>Saját, elkülönített memóriabeli adatbázis; a gyártó minden hívásra új DbContextet ad ugyanahhoz az adatbázishoz.</summary>
        public static IDbContextFactory<TeszterLocalDB> CreateDbFactory(params Group[] groups)
        {
            var options = new DbContextOptionsBuilder<TeszterLocalDB>()
                .UseInMemoryDatabase("teszter-tests-" + Guid.NewGuid().ToString("N"))
                .Options;
            var factory = new PooledDbContextFactory<TeszterLocalDB>(options);
            using var db = factory.CreateDbContext();
            db.Groups.AddRange(groups);
            db.SaveChanges();
            return factory;
        }
    }
}
