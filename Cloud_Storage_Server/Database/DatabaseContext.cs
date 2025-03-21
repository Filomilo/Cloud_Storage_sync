using Cloud_Storage_Server.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using FileData = Cloud_Storage_Server.Database.Models.FileData;

namespace Cloud_Storage_Server.Database
{
    public class DatabaseContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FileData> Files { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseInMemoryDatabase("cloud_storage");
            optionsBuilder.UseSqlite("Data Source=databse.sqllite");
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileData>()
                .HasIndex(f => new { f.Extenstion, f.Name, f.Path,f.OwnerId })
                .IsUnique();
        }


    }
}
