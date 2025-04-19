using Cloud_Storage_Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Server.Database
{
    public class DatabaseContextSqLite : AbstractDataBaseContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseInMemoryDatabase("cloud_storage");
            optionsBuilder.UseSqlite("Data Source=databse1.db");
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //throw new NotImplementedException();
            //modelBuilder
            //    .Entity<SyncFileData>()
            //    .HasIndex(f => new
            //    {
            //        f.Extenstion,
            //        f.Name,
            //        f.Path,
            //        f.OwnerId,
            //        f.
            //    })
            //    .IsUnique();
        }
    }

    public class SqliteDataBaseContextGenerator : IDataBaseContextGenerator
    {
        public AbstractDataBaseContext GetDbContext()
        {
            return new DatabaseContextSqLite();
        }
    }
}
