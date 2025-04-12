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
    public class FileRepositoryService : IFileRepositoryService
    {
        private IDbContextGenerator _dbContextGenerator;

        public FileRepositoryService(IDbContextGenerator dbContextGenerator)
        {
            _dbContextGenerator = dbContextGenerator;
        }

        public AbstractDataBaseContext GetDbContext()
        {
            return _dbContextGenerator.GetDbContext();
        }

        public void AddNewFile(LocalFileData file)
        {
            using (var context = GetDbContext())
            {
                context.Files.Add(file);
                context.SaveChanges();
            }
        }

        public void DeleteFile(LocalFileData file)
        {
            using (var context = GetDbContext())
            {
                context.Files.Remove(file);
                context.SaveChanges();
            }
        }

        public void UpdateFile(LocalFileData oldFileData, LocalFileData newFileData)
        {
            using (var context = GetDbContext())
            {
                context.Files.Remove(oldFileData);
                context.Files.Add(newFileData);
                context.SaveChanges();
            }
        }

        public IEnumerable<LocalFileData> GetAllFiles()
        {
            using (var context = GetDbContext())
            {
                return context.Files.ToList();
            }
        }

        public void Reset()
        {
            using (var context = GetDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
    }
}
