using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using FileData = Cloud_Storage_Common.Models.FileData;

namespace Cloud_Storage_Server.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<SyncFileData> Files { get; set; }
        public DbSet<Device> Devices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseInMemoryDatabase("cloud_storage");
            optionsBuilder.UseSqlite("Data Source=databse1.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //throw new NotImplementedException();
            modelBuilder
                .Entity<SyncFileData>()
                .HasIndex(f => new
                {
                    f.Extenstion,
                    f.Name,
                    f.Path,
                    f.OwnerId,
                })
                .IsUnique();
        }

        internal Device Find(string id)
        {
            throw new NotImplementedException();
        }
    }
}
