using System.ComponentModel.DataAnnotations;
using System.Transactions;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Handlers;
using Cloud_Storage_Server.Interfaces;

namespace Cloud_Storage_Server.Services
{
    public class FileUploadRequest
    {
        public SyncFileData syncFileData;
        public Stream fileStream;

        public FileUploadRequest(SyncFileData syncFileData, Stream fileStream)
        {
            this.syncFileData = syncFileData;
            this.fileStream = fileStream;
        }
    }

    public delegate void FileUpdateHandler(SyncFileData uploudFile);

    public interface IFileSyncService
    {
        public void AddNewFile(User user, string deviceId, UploudFileData data, Stream file);
        public Stream DownloadFile(User user, SyncFileData data);
        public List<SyncFileData> ListFilesForUser(User user);
        public bool DoesFileAlreadyExist(User user, UploudFileData data);
        event FileUpdateHandler FileUpdated;
    }

    public class FileSyncService : IFileSyncService
    {
        private IFileSystemService _fileSystemService;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("FileSyncService");
        private IHandler _AddNewFileHandler;

        public FileSyncService(
            IFileSystemService fileSystemService,
            IWebsocketConnectedController websocketConnectedController
        )
        {
            _fileSystemService = fileSystemService;
            _AddNewFileHandler = new SkipIfTheSameFileAlreadyExist();
            _AddNewFileHandler
                .SetNext(new UpdateIfOnlyOwnerChanged())
                .SetNext(new SaveAndUpdateNewVersionOfFile(this._fileSystemService));
            this.FileUpdated += (SyncFileData file) =>
            {
                websocketConnectedController.SendMessageToUser(
                    file.OwnerId,
                    new WebSocketMessage(file)
                );
            };
        }

        public void AddNewFile(User user, string deviceId, UploudFileData data, Stream file)
        {
            SyncFileData fileData = new SyncFileData(data);
            fileData.OwnerId = user.id;
            fileData.DeviceOwner = new List<string>();
            fileData.DeviceOwner.Add(deviceId);
            FileUploadRequest fileUploadRequest = new FileUploadRequest(fileData, file);
            try
            {
                SyncFileData sync = (SyncFileData)_AddNewFileHandler.Handle(fileUploadRequest);
                if (FileUpdated != null)
                {
                    FileUpdated.Invoke(sync);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error saving file to server: [[]{ex.Message}]");
                throw ex;
            }
        }

        public bool DoesFileAlreadyExist(User user, UploudFileData data)
        {
            try
            {
                SyncFileData fileInRepo = FileRepository.getFileByPathNameExtensionAndUser(
                    data.Path,
                    data.Name,
                    data.Extenstion,
                    user.id
                );
                if (fileInRepo == null || fileInRepo.Hash == data.Hash)
                {
                    return true;
                }
            }
            catch (KeyNotFoundException ex)
            {
                return false;
            }
            return false;
        }

        public event FileUpdateHandler? FileUpdated;

        private static string GetRealtivePathForFile(User user, SyncFileData data)
        {
            return $"{user.id}\\{data.Id}";
        }

        public Stream DownloadFile(User user, SyncFileData data)
        {
            Stream RawData = this._fileSystemService.GetFile(GetRealtivePathForFile(user, data));
            return RawData;
        }

        public List<SyncFileData> ListFilesForUser(User user)
        {
            List<SyncFileData> files = FileRepository.GetAllUserFiles(user.id);
            return files;
        }
    }
}
