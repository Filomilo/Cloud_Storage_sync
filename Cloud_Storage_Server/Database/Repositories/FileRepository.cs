using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common.Models;
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

        public static SyncFileData getFileByPathNameExtensionAndUser(
            string path,
            string name,
            string extension,
            long ownerId
        )
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                SyncFileData file = context.Files.FirstOrDefault(x =>
                    x.Path == path
                    && x.Name == name
                    && x.Extenstion == extension
                    && x.OwnerId == ownerId
                );
                if (file == null)
                    throw new KeyNotFoundException(
                        "File specofied by provieded paramaters doesnt exists"
                    );

                return file;
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

        internal static void UpdateFile(FileData fileToSave, FileData fileUpdateData)
        {
            throw new NotImplementedException();
        }
    }
}
