using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cloud_Storage_Desktop_lib.Database
{
    class InMemoryDataBase : AbstractDataBaseContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Files");
            //optionsBuilder.UseSqlite(
            //    "Data Source="
            //        + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            //        + "Files.db"
            //);
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(InMemoryEventId.TransactionIgnoredWarning)
            );
        }
    }

    public class InMemoryDataBAseContextGenerator : IDbContextGenerator
    {
        public AbstractDataBaseContext GetDbContext()
        {
            return new InMemoryDataBase();
        }
    }
}
