using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class FileRepositoryService : IFileRepositoryService
    {
        private object _locker = new object();
        private IDbContextGenerator _dbContextGenerator;
        private ILogger Logger = CloudDriveLogging.Instance.GetLogger("FileRepositoryService");

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

        public void DeleteFileByPath(string realitveDirectory, string name, string extesnion)
        {
            lock (_locker)
            {
                using (var context = GetDbContext())
                {
                    LocalFileData local = context
                        .Files.Where(f =>
                            f.Path == realitveDirectory
                            && f.Name == name
                            && f.Extenstion == extesnion
                        )
                        .FirstOrDefault();
                    if (local == null)
                    {
                        throw new ArgumentException(
                            $"Cannot find file in repository to remove {realitveDirectory}{name}{extesnion}"
                        );
                    }
                    Logger.LogInformation($"Removing from database file: {local}");
                    context.Files.Remove(local);
                    context.SaveChanges();
                }
            }
        }

        public LocalFileData GetFileByPathNameExtension(string path, string name, string extenstion)
        {
            lock (_locker)
            {
                using (var context = GetDbContext())
                {
                    LocalFileData local = context
                        .Files.Where(f =>
                            f.Path == path && f.Name == name && f.Extenstion == extenstion
                        )
                        .FirstOrDefault();

                    return local;
                }
            }
        }
    }
}
