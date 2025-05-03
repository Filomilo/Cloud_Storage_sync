using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Database
{
    public enum DBTYPE
    {
        INMEM,
        SQLLITE,
    }

    public static class DbContextGeneratorFactory
    {
        public static DBTYPE dbType { get; set; } = DBTYPE.SQLLITE;

        public static IDbContextGenerator GetContextGenerator()
        {
            switch (dbType)
            {
                case DBTYPE.INMEM:
                    return new InMemoryDataBAseContextGenerator();
                case DBTYPE.SQLLITE:
                    return new LocalSqlLiteDbGeneraor();
            }

            throw new ArgumentException("db type ont hadnles");
        }
    }
}
