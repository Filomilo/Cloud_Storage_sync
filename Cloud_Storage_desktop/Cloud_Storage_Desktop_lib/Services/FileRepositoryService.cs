using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Desktop_lib.Services
{
    class DataBaseContext : DbContext
    {
        public DbSet<LocalFileData> Files { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Files");
            //optionsBuilder.UseSqlite(
            //    "Data Source="
            //        + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            //        + "Files.db"
            //);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //throw new NotImplementedException();
            modelBuilder
                .Entity<LocalFileData>()
                .HasIndex(f => new
                {
                    f.Extenstion,
                    f.Name,
                    f.Path,
                })
                .IsUnique();
        }
    }

    public class FileRepositoryService : IFileRepositoryService
    {
        public void AddNewFile(LocalFileData file)
        {
            using (var context = new DataBaseContext())
            {
                context.Files.Add(file);
                context.SaveChanges();
            }
        }

        public void DeleteFile(LocalFileData file)
        {
            using (var context = new DataBaseContext())
            {
                context.Files.Remove(file);
                context.SaveChanges();
            }
        }

        public void UpdateFile(LocalFileData oldFileData, LocalFileData newFileData)
        {
            using (var context = new DataBaseContext())
            {
                context.Files.Remove(oldFileData);
                context.Files.Add(newFileData);
                context.SaveChanges();
            }
        }

        public IEnumerable<LocalFileData> GetAllFiles()
        {
            using (var context = new DataBaseContext())
            {
                return context.Files.ToList();
            }
        }

        public void Reset()
        {
            using (var context = new DataBaseContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
    }
}
