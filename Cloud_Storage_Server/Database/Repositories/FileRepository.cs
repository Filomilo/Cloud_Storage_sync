using Cloud_Storage_Server.Database.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using File = Cloud_Storage_Server.Database.Models.File;

namespace Cloud_Storage_Server.Database.Repositories
{
    public static class FileRepository
    {
        public static File SaveNewFile(File file)
        {
            File savedFile = null;
            using (DatabaseContext context = new DatabaseContext())
            {
                var validationContext = new ValidationContext(file);
                Validator.ValidateObject(file, validationContext, true);
                savedFile=context.Files.Add(file).Entity;
                context.SaveChanges();
            }
            return savedFile;
        }

        public static File  UpdateFile(File previousFileDataa,File newFileData)
        {
            File updatedFile;
            using (DatabaseContext context = new DatabaseContext())
            {
                var validationContext = new ValidationContext(newFileData);
                Validator.ValidateObject(newFileData, validationContext, true);
                File found= context.Files.FirstOrDefault(
                    x => x.Path == previousFileDataa.Path && x.Name == previousFileDataa.Name && x.Extenstion == previousFileDataa.Extenstion
                );
                if (found == null)
                {
                    throw new KeyNotFoundException("No File like provided to update");
;                }
                
                found.Path = newFileData.Path;
                found.Extenstion = newFileData.Extenstion;
                found.Hash= newFileData.Hash;
                found.SyncDate=newFileData.SyncDate;
                found.Name=newFileData.Name;
                updatedFile=context.Files.Update(found).Entity;
                context.SaveChanges();
            }
            return updatedFile; 
        }


        public static File GetFileOfID(Guid id)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
               File file= context.Files.Find(id);
               if (file == null)
                   throw new KeyNotFoundException("No file iwth this guuid");
               return file;
            }
        }

        public static File getFileByPathNameExtensionAndUser(
            string path,
            string name,
            string extension,
            long ownerId
        ){
        using (DatabaseContext context = new DatabaseContext())
        {
                File file = context.Files.FirstOrDefault(
                                                    x =>
                                                        x.Path == path &&
                                                        x.Name == name &&
                                                        x.Extenstion == extension &&
                                                         x.OwnerId == ownerId
                                                        
                                                            );
                                                            if (file == null)
                                                                throw new KeyNotFoundException("File specofied by provieded paramaters doesnt exists");

            return file;
        }

        }


        public static List<File> GetAllUserFiles(long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<File> files = context.Files.Where(x => x.OwnerId == userId).ToList();
                return files;
            }
        }
    }
}
