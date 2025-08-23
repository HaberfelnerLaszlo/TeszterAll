using Microsoft.EntityFrameworkCore;
using TeszterLibrary.Model;
namespace TeszterWeb.Data
{
    public class TeszterLocalDB: DbContext
    {
        public TeszterLocalDB(DbContextOptions<TeszterLocalDB> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any additional model configurations here
        }
        // Define DbSets for your entities here
        public DbSet<Group> Groups { get; set; }
    }
}
