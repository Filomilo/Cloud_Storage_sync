using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using FileData = Cloud_Storage_Common.Models.FileData;

namespace Cloud_Storage_Server.Database.Repositories
{
    public static class FileRepository
    {
        public static SyncFileData SaveNewFile(SyncFileData file)
        {
            SyncFileData savedFile = null;
            using (DatabaseContext context = new DatabaseContext())
            {
                var validationContext = new ValidationContext(file);
                Validator.ValidateObject(file, validationContext, true);
                savedFile = context.Files.Add(file).Entity;
                context.SaveChanges();
            }
            return savedFile;
        }

        public static SyncFileData GetFileOfID(Guid id)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                SyncFileData file = context.Files.Find(id);
                if (file == null)
                    throw new KeyNotFoundException("No file iwth this guuid");
                return file;
            }
        }

        public static IEnumerable<SyncFileData> getFileByPathNameExtensionAndUser(
            string path,
            string name,
            string extension,
            long ownerId
        )
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                IEnumerable<SyncFileData> files = context.Files.Where(x =>
                    x.Path == path
                    && x.Name == name
                    && x.Extenstion == extension
                    && x.OwnerId == ownerId
                );

                return files;
            }
        }

        public static List<SyncFileData> GetAllUserFiles(long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<SyncFileData> files = context.Files.Where(x => x.OwnerId == userId).ToList();
                return files;
            }
        }

        internal static void UpdateFile(SyncFileData fileInRepositry, SyncFileData fileUpdateData)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                SyncFileData file = context.Files.Find(fileInRepositry.Id);
                file.Name = fileUpdateData.Name;
                file.Path = fileUpdateData.Path;
                file.Extenstion = fileUpdateData.Extenstion;
                file.Hash = fileUpdateData.Hash;
                file.DeviceOwner = fileUpdateData.DeviceOwner;
                file.SyncDate = fileUpdateData.SyncDate;
                file.Version = fileUpdateData.Version;
                context.Files.Update(file);
                context.SaveChanges();
            }
        }

        internal static SyncFileData getNewestFileByPathNameExtensionAndUser(
            string path,
            string name,
            string extenstion,
            long ownerId
        )
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                SyncFileData file = context
                    .Files.Where(x =>
                        x.Path == path
                        && x.Name == name
                        && x.Extenstion == extenstion
                        && x.OwnerId == ownerId
                    )
                    .ToList()
                    .OrderBy(x => x.Version)
                    .FirstOrDefault();

                return file;
            }
        }

        public static SyncFileData getFileByPathNameExtensionUserAndDeviceOwner(
            string path,
            string name,
            string extenstion,
            long ownerId,
            string deviceID
        )
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                SyncFileData file = context.Files.FirstOrDefault(x =>
                    x.Path == path
                    && x.Name == name
                    && x.Extenstion == extenstion
                    && x.OwnerId == ownerId
                    && x.DeviceOwner.Contains(deviceID)
                );

                return file;
            }
        }
    }
}
