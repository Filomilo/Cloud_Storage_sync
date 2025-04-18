using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    public class UpdateDataBaseFileNameHandler : AbstactHandler
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger(
            "UpdateDataBaseFileNameHandler"
        );
        IFileRepositoryService _fileRepositoryService;
        IConfiguration _configuration;

        public UpdateDataBaseFileNameHandler(
            IFileRepositoryService repository,
            IConfiguration configuration
        )
        {
            _fileRepositoryService = repository;
            _configuration = configuration;
        }

        public override object Handle(object request)
        {
            if (request is not RenamedEventArgs)
                throw new ArgumentException(
                    "ValidateIfFileAlreadyExisitInDataBase excepts argument of type SyncFileData"
                );

            RenamedEventArgs renamedEventArgs = request as RenamedEventArgs;
            string oldPath = renamedEventArgs.OldFullPath;
            string newPath = renamedEventArgs.FullPath;

            FileManager.GetFilePathParamsFormRelativePath(
                FileManager.GetRealtiveFullPathToFile(oldPath, _configuration.StorageLocation),
                out string oldDir,
                out string oldName,
                out string oldExtesnion
            );

            FileManager.GetFilePathParamsFormRelativePath(
                FileManager.GetRealtiveFullPathToFile(newPath, _configuration.StorageLocation),
                out string newDir,
                out string newName,
                out string newExtesnion
            );
            LocalFileData newFileData = null;
            LocalFileData oldFileDataCopy = null;
            using (var ctx = this._fileRepositoryService.GetDbContext())
            {
                LocalFileData oldFileData = ctx
                    .Files.Where(x =>
                        x.Path.Equals(oldDir)
                        && x.Name.Equals(oldName)
                        && x.Extenstion.Equals(oldExtesnion)
                    )
                    .FirstOrDefault();

                oldFileDataCopy = oldFileData == null ? null : oldFileData.Clone();

                if (oldFileData != null)
                {
                    ctx.Files.Remove(oldFileData);
                    ctx.SaveChanges();

                    oldFileData.Name = newName;
                    oldFileData.Extenstion = newExtesnion;
                    oldFileData.Path = newDir;
                    oldFileData.Version++;
                    ctx.Files.Add(oldFileData);
                    newFileData = oldFileData;
                }

                ctx.SaveChanges();
            }

            UpdateFileDataRequest updateFileDataRequest = new UpdateFileDataRequest()
            {
                oldFileData = oldFileDataCopy == null ? null : new SyncFileData(oldFileDataCopy),
                newFileData = newFileData == null ? null : new SyncFileData(newFileData),
            };

            if (
                this._nextHandler != null
                && updateFileDataRequest != null
                && updateFileDataRequest.newFileData != null
            )
            {
                return this._nextHandler.Handle(updateFileDataRequest);
            }
            return updateFileDataRequest;
        }
    }
}
