using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using FileData = Cloud_Storage_Common.Models.FileData;

namespace Cloud_Storage_Server.Database.Repositories
{
    public static class FileRepository
    {
        public static FileData SaveNewFile(FileData file)
        {
            throw new NotImplementedException();

            //FileData savedFile = null;
            //using (DatabaseContext context = new DatabaseContext())
            //{
            //    var validationContext = new ValidationContext(file);
            //    Validator.ValidateObject(file, validationContext, true);
            //    savedFile = context.Files.Add(file).Entity;
            //    context.SaveChanges();
            //}
            //return savedFile;
        }

        public static FileData UpdateFile(FileData previousFileDataa, FileData newFileData)
        {
            FileData updatedFile;
            //            using (DatabaseContext context = new DatabaseContext())
            //            {
            //                var validationContext = new ValidationContext(newFileData);
            //                Validator.ValidateObject(newFileData, validationContext, true);
            //                FileData found= context.Files.FirstOrDefault(
            //                    x => x.Path == previousFileDataa.Path && x.Name == previousFileDataa.Name && x.Extenstion == previousFileDataa.Extenstion
            //                );
            //                if (found == null)
            //                {
            //                    throw new KeyNotFoundException("No File like provided to update");
            //;                }

            //                found.Path = newFileData.Path;
            //                found.Extenstion = newFileData.Extenstion;
            //                found.Hash= newFileData.Hash;
            //                found.SyncDate=newFileData.SyncDate;
            //                found.Name=newFileData.Name;
            //                updatedFile=context.Files.Update(found).Entity;
            //                context.SaveChanges();
            //            }
            //            return updatedFile;
            throw new NotImplementedException();
        }

        public static FileData GetFileOfID(Guid id)
        {
            throw new NotImplementedException();

            //using (DatabaseContext context = new DatabaseContext())
            //{
            //    FileData file = context.Files.Find(id);
            //    if (file == null)
            //        throw new KeyNotFoundException("No file iwth this guuid");
            //    return file;
            //}
        }

        public static FileData getFileByPathNameExtensionAndUser(
            string path,
            string name,
            string extension,
            long ownerId
        )
        {
            //using (DatabaseContext context = new DatabaseContext())
            //{
            //        FileData file = context.Files.FirstOrDefault(
            //                                            x =>
            //                                                x.Path == path &&
            //                                                x.Name == name &&
            //                                                x.Extenstion == extension &&
            //                                                 x.OwnerId == ownerId

            //                                                    );
            //                                                    if (file == null)
            //                                                        throw new KeyNotFoundException("File specofied by provieded paramaters doesnt exists");

            //    return file;
            //}
            throw new NotImplementedException();
        }

        public static List<SyncFileData> GetAllUserFiles(long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<SyncFileData> files = context.Files.Where(x => x.OwnerId == userId).ToList();
                return files;
            }
        }
    }
}
