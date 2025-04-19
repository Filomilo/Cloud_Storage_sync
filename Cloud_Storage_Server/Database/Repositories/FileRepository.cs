using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Server.Database.Repositories
{
    public static class FileRepository
    {
        public static SyncFileData SaveNewFile(AbstractDataBaseContext context, SyncFileData file)
        {
            SyncFileData savedFile = null;

            var validationContext = new ValidationContext(file);
            Validator.ValidateObject(file, validationContext, true);
            savedFile = context.Files.Add(file).Entity;
            context.SaveChanges();

            return savedFile;
        }

        public static SyncFileData GetFileOfID(AbstractDataBaseContext context, Guid id)
        {
            SyncFileData file = context.Files.Where(x => x.Id.Equals(id)).FirstOrDefault();
            if (file == null)
                throw new KeyNotFoundException("No file iwth this guuid");
            return file;
        }

        public static IEnumerable<SyncFileData> getFileByPathNameExtensionAndUser(
            AbstractDataBaseContext context,
            string path,
            string name,
            string extension,
            long ownerId
        )
        {
            IEnumerable<SyncFileData> files = context.Files.Where(x =>
                x.Path == path
                && x.Name == name
                && x.Extenstion == extension
                && x.OwnerId == ownerId
            );

            return files;
        }

        public static List<SyncFileData> GetAllUserFiles(
            AbstractDataBaseContext context,
            long userId
        )
        {
            List<SyncFileData> files = context.Files.Where(x => x.OwnerId == userId).ToList();
            return files;
        }

        internal static void UpdateFile(
            AbstractDataBaseContext context,
            SyncFileData fileInRepositry,
            SyncFileData fileUpdateData
        )
        {
            SyncFileData file = context
                .Files.Where(x => x.Id.Equals(fileInRepositry.Id))
                .FirstOrDefault();
            if (file == null)
                throw new KeyNotFoundException("No file iwth this guuid");
            context.Remove(file);
            fileUpdateData.Id = file.Id;
            context.Files.Add(fileUpdateData);
            context.SaveChanges();
        }

        internal static SyncFileData getNewestFileByPathNameExtensionAndUser(
            AbstractDataBaseContext context,
            string path,
            string name,
            string extenstion,
            long ownerId
        )
        {
            SyncFileData file = context
                .Files.Where(x =>
                    x.Path == path
                    && x.Name == name
                    && x.Extenstion == extenstion
                    && x.OwnerId == ownerId
                )
                .ToList()
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();

            return file;
        }

        public static SyncFileData getFileByPathNameExtensionUserAndDeviceOwner(
            AbstractDataBaseContext context,
            string path,
            string name,
            string extenstion,
            long ownerId,
            string deviceID
        )
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
