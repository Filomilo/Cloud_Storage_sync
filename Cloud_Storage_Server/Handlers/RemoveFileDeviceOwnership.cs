using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Interfaces;

namespace Cloud_Storage_Server.Handlers
{
    public class RemoveFileDeviceOwnershipRequest
    {
        public FileData fileData;
        public SyncFileData syncFileData;
        public long userID;
        public string deviceId;
    }

    public class RemoveFileDeviceOwnership : AbstactHandler
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator;
        private static ILogger _logger = CloudDriveLogging.Instance.GetLogger(
            "RemoveFileDeviceOwnership"
        );

        public RemoveFileDeviceOwnership(IDataBaseContextGenerator dataBaseContextGenerator)
        {
            _dataBaseContextGenerator = dataBaseContextGenerator;
        }

        public override object Handle(object request)
        {
            if (request is not RemoveFileDeviceOwnershipRequest)
            {
                throw new ArgumentException(
                    "RemoveFileDeviceOwnership excepts argument of type RemoveFileDeviceOwnershipRequest"
                );
            }
            RemoveFileDeviceOwnershipRequest removeFileDeviceOwnership =
                request as RemoveFileDeviceOwnershipRequest;
            SyncFileData newFile = null;
            using (AbstractDataBaseContext context = _dataBaseContextGenerator.GetDbContext())
            {
                var existingFile = GetExisitingFileOnThisDeviceFromDatabse(
                    context,
                    removeFileDeviceOwnership
                );

                if (existingFile.Hash == "")
                {
                    if (this._nextHandler != null)
                    {
                        return this._nextHandler.Handle(existingFile);
                    }
                    return null;
                }

                var exisitingDeletedFile = GetAlreadyDletedFileVersionFromDataBase(
                    context,
                    removeFileDeviceOwnership,
                    existingFile
                );

                if (exisitingDeletedFile == null)
                {
                    existingFile.DeviceOwner.Remove(removeFileDeviceOwnership.deviceId);
                    context.Files.Update(existingFile);
                    newFile = existingFile.Clone();
                    newFile.DeviceOwner = new List<string>() { removeFileDeviceOwnership.deviceId };
                    newFile.Version = newFile.Version + 1;
                    newFile.Hash = "";
                    newFile.Id = Guid.NewGuid();
                    context.Files.Add(newFile);
                }
                else
                {
                    exisitingDeletedFile.DeviceOwner.Add(removeFileDeviceOwnership.deviceId);
                    context.Files.Update(exisitingDeletedFile);
                    newFile = exisitingDeletedFile;
                }

                existingFile.DeviceOwner.Remove(removeFileDeviceOwnership.deviceId);
                context.Files.Update(existingFile);

                context.SaveChangesAsync().Wait();
            }

            if (newFile == null)
            {
                throw new Exception("Filed to save new file data");
            }
            removeFileDeviceOwnership.syncFileData = newFile;

            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(removeFileDeviceOwnership);
            }
            return removeFileDeviceOwnership;
        }

        private bool StopIFileLAlreadyDletedForThisDevice(
            SyncFileData? existingFile,
            AbstractDataBaseContext context,
            out object syncFileData
        )
        {
            if (existingFile.Hash == "")
            {
                context.SaveChangesAsync().Wait();
                if (this._nextHandler != null)
                {
                    syncFileData = this._nextHandler.Handle(existingFile);
                    return true;
                }

                syncFileData = existingFile;
                return true;
            }
            syncFileData = null;
            return false;
        }

        private static SyncFileData? GetAlreadyDletedFileVersionFromDataBase(
            AbstractDataBaseContext context,
            RemoveFileDeviceOwnershipRequest? removeFileDeviceOwnership,
            SyncFileData? existingFile
        )
        {
            SyncFileData exisitingDeletedFile = context
                .Files.ToList()
                .Where(x =>
                    x.GetRealativePath()
                        .Equals(removeFileDeviceOwnership.fileData.GetRealativePath())
                    && x.OwnerId == removeFileDeviceOwnership.userID
                    && x.Version > existingFile.Version
                )
                .FirstOrDefault();
            return exisitingDeletedFile;
        }

        private static SyncFileData? GetExisitingFileOnThisDeviceFromDatabse(
            AbstractDataBaseContext context,
            RemoveFileDeviceOwnershipRequest? removeFileDeviceOwnership
        )
        {
            _logger.LogTrace(
                $"Searhing in db for ilfe with path: {removeFileDeviceOwnership.fileData.GetRealativePath()} and owner id: {removeFileDeviceOwnership.userID} , deviceo wner :{removeFileDeviceOwnership.deviceId}"
            );
            _logger.LogTrace(
                $"Searhing in db for ilfe with path: {removeFileDeviceOwnership.fileData.Path} and owner id: {removeFileDeviceOwnership.userID} , deviceo wner :{removeFileDeviceOwnership.deviceId}"
            );
            _logger.LogTrace(
                $"Searhing in db with content: \n\n [[\n {String.Join(",\n", context
                    .Files.ToList().Select(x=> $"GetRealativePathWindowsStyle: [[[{x.GetRealativePath()};; OwnerId: {x.OwnerId};;DeviceOwner: [{String.Join(", ",x.DeviceOwner )}]  ]]]"))}\n]]\n"
            );
            _logger.LogTrace(
                $"Searhing in db with content: \n\n [[\n {String.Join(",\n", context
                  .Files.ToList().Select(x => $"path: [[[{x.Path};; OwnerId: {x.OwnerId};;DeviceOwner: [{String.Join(", ", x.DeviceOwner)}]  ]]]"))}\n]]\n"
            );

            SyncFileData existingFile = context
                .Files.ToList()
                .Where(x =>
                    x.GetRealativePath()
                        .Equals(removeFileDeviceOwnership.fileData.GetRealativePath())
                    && x.OwnerId.Equals(removeFileDeviceOwnership.userID)
                    && x.DeviceOwner.Contains(removeFileDeviceOwnership.deviceId)
                )
                .FirstOrDefault();
            if (existingFile == null)
            {
                throw new KeyNotFoundException(
                    $"File with path {removeFileDeviceOwnership.fileData.GetRealativePath()} not found"
                );
            }

            return existingFile;
        }
    }
}
