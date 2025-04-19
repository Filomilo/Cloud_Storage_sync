using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Server.Database
{
    public abstract class AbstractDataBaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<SyncFileData> Files { get; set; }
        public DbSet<Device> Devices { get; set; }
    }
}
