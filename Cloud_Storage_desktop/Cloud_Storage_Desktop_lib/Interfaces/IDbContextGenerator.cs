using Cloud_Storage_Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public abstract class AbstractDataBaseContext : DbContext
    {
        public DbSet<LocalFileData> Files { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseInMemoryDatabase("Files");
        //    //optionsBuilder.UseSqlite(
        //    //    "Data Source="
        //    //        + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        //    //        + "Files.db"
        //    //);
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    //throw new NotImplementedException();
        //    modelBuilder
        //        .Entity<LocalFileData>()
        //        .HasIndex(f => new
        //        {
        //            f.Extenstion,
        //            f.Name,
        //            f.Path,
        //        })
        //        .IsUnique();
        //}
    }

    public interface IDbContextGenerator
    {
        public AbstractDataBaseContext GetDbContext();
    }
}
