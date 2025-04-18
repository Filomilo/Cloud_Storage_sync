using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Services;

namespace Cloud_Storage_Server.Handlers
{
    public class SaveAndUpdateNewVersionOfFile : AbstactHandler
    {
        private IFileSystemService _fileSystemService;

        public SaveAndUpdateNewVersionOfFile(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        private static string GetRealtivePathForFile(long userid, SyncFileData data)
        {
            return $"{userid}\\{data.Id}";
        }

        public override object Handle(object request)
        {
            if (request.GetType() != typeof(FileUploadRequest))
            {
                throw new ArgumentException(
                    "SkipIfTheSameFileAreadyExist excepts argument of type FileUploadRequest"
                );
            }

            FileUploadRequest fileUploadRequest = (FileUploadRequest)request;
            SyncFileData uploudFileData = fileUploadRequest.syncFileData;

            using (DatabaseContext context = new DatabaseContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    SyncFileData file = (SyncFileData)fileUploadRequest.syncFileData;
                    var validationContext = new ValidationContext(file);
                    Validator.ValidateObject(file, validationContext, true);

                    SyncFileData saved = context.Files.Add(file).Entity;
                    context.SaveChanges();

                    this._fileSystemService.SaveFile(
                        GetRealtivePathForFile(saved.OwnerId, saved),
                        fileUploadRequest.fileStream
                    );

                    transaction.Commit();
                    if (_nextHandler != null)
                    {
                        return _nextHandler.Handle(saved);
                    }
                    return saved;
                }
            }
        }
    }
}
