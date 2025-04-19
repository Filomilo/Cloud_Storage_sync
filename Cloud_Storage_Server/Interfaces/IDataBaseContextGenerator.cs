using Cloud_Storage_Server.Database;

namespace Cloud_Storage_Server.Interfaces
{
    public interface IDataBaseContextGenerator
    {
        public AbstractDataBaseContext GetDbContext();
    }
}
