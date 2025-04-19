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
            SyncFileData saved = null;
            using (DatabaseContext context = new DatabaseContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    SyncFileData file = (SyncFileData)fileUploadRequest.syncFileData;
                    var validationContext = new ValidationContext(file);
                    Validator.ValidateObject(file, validationContext, true);

                    if (
                        this.getNewestVersionOfTheSameFile(
                            context,
                            file,
                            out SyncFileData newestVersionAlreadyInDataBase
                        )
                    )
                    {
                        file.Version = newestVersionAlreadyInDataBase.Version + 1;
                        if (
                            newestVersionAlreadyInDataBase.DeviceOwner.Contains(
                                file.DeviceOwner.First()
                            )
                        )
                        {
                            newestVersionAlreadyInDataBase.DeviceOwner.Remove(
                                file.DeviceOwner.FirstOrDefault()
                            );
                            context.Files.Update(newestVersionAlreadyInDataBase);
                        }
                    }

                    saved = context.Files.Add(file).Entity;

                    this._fileSystemService.SaveFile(
                        GetRealtivePathForFile(saved.OwnerId, saved),
                        fileUploadRequest.fileStream
                    );

                    transaction.Commit();
                    context.SaveChanges();
                }
            }
            if (_nextHandler != null && saved != null)
            {
                return _nextHandler.Handle(saved);
            }
            return saved;
        }

        private bool getNewestVersionOfTheSameFile(
            DatabaseContext context,
            SyncFileData file,
            out SyncFileData newestVersionAlreadyInDataBase
        )
        {
            newestVersionAlreadyInDataBase = context
                .Files.ToList()
                .Where(f =>
                    f.GetRealativePath().Equals(file.GetRealativePath())
                    && f.OwnerId == file.OwnerId
                )
                .OrderByDescending(f => f.Version)
                .FirstOrDefault();
            return newestVersionAlreadyInDataBase != null;
        }
    }
}
