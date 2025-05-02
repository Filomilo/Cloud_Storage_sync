using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Desktop_lib.Database
{
    internal class SqlLieLocalFbContext : AbstractDataBaseContext
    {
        private string GetConnString()
        {
            return $"Data Source={Path.Join(SharedData.GetAppDirectory(), "database.db")};";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(GetConnString());
        }
    }

    public class LocalSqlLiteDbGeneraor : IDbContextGenerator
    {
        public AbstractDataBaseContext GetDbContext()
        {
            SqlLieLocalFbContext ctx = new SqlLieLocalFbContext();
            ctx.Database.EnsureCreated();
            return ctx;
        }
    }
}
