using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.EntityFrameworkCore;

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
