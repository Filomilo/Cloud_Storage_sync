using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Controllers;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;

namespace Cloud_Storage_Server.Handlers
{
    public class ClearBackupsOverload : AbstactHandler
    {
        private ILogger _logger = CloudDriveLogging.Instance.GetLogger("ClearBackupsOverload");
        private IDataBaseContextGenerator _dataBaseContextGenerator;
        private IServerConfig _serverConfig;
        private IFileSystemService _fileSystemService;

        public ClearBackupsOverload(
            IDataBaseContextGenerator dataBaseContextGenerator,
            IServerConfig serverConfig,
            IFileSystemService fileSystemService
        )
        {
            _dataBaseContextGenerator = dataBaseContextGenerator;
            _serverConfig = serverConfig;
            _fileSystemService = fileSystemService;
        }

        public override object Handle(object request)
        {
            SyncFileData syncFileData = null;
            if (request is SyncFileData)
            {
                syncFileData = request as SyncFileData;
            }

            if (request is UpdateFileDataRequest)
            {
                syncFileData = (request as UpdateFileDataRequest).newFileData;
            }

            if (syncFileData == null)
            {
                throw new ArgumentException(
                    "ClearBackupsOverload excepts argument of type SyncFileData"
                );
            }

            using (AbstractDataBaseContext context = _dataBaseContextGenerator.GetDbContext())
            {
                bool didFished = false;
                while (!didFished)
                {
                    didFished = true;
                    List<SyncFileData> allFileVersion = FileRepository
                        .getFileByPathNameExtensionAndUser(
                            context,
                            syncFileData.Path,
                            syncFileData.Name,
                            syncFileData.Extenstion,
                            syncFileData.OwnerId
                        )
                        .ToList();
                    long allFileVersionSize = allFileVersion.Select(x => x.BytesSize).Sum();
                    if (allFileVersionSize <= _serverConfig.BackupMaxSize)
                    {
                        didFished = true;
                        break;
                    }
                    SyncFileData fileToRemove = allFileVersion
                        .Where(x => x.DeviceOwner.Count == 0)
                        .OrderBy(x => x.Version)
                        .FirstOrDefault();
                    if (fileToRemove == null)
                    {
                        didFished = true;
                        break;
                    }
                    _logger.LogInformation($"Removing file {fileToRemove}");
                    FileRepository.RemoveFile(context, fileToRemove);
                    _fileSystemService.DeleteFile(fileToRemove);
                }

                context.SaveChangesAsync().Wait();
            }

            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(request);
            }
            return request;
        }
    }
}
