using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
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

    public delegate void FileUpdateHandler(UpdateFileDataRequest uploudFile);

    public interface IFileSyncService
    {
        public void AddNewFile(User user, string deviceId, UploudFileData data, Stream file);
        public Stream DownloadFile(User user, SyncFileData data);
        public List<SyncFileData> ListFilesForUser(User user);
        public bool DoesFileAlreadyExist(User user, UploudFileData data);
        void RemoveFile(FileData fileData, long id, string deviceId);

        event FileUpdateHandler FileUpdated;
        void UpdateFileForDevice(string email, string deviceId, UpdateFileDataRequest file);
    }

    public class FileSyncService : IFileSyncService
    {
        private IFileSystemService _fileSystemService;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("FileSyncService");
        private IHandler _AddNewFileHandler;
        private IHandler _RemoveFileHandler;
        private IHandler _UpdateFileHandler;

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
            this.FileUpdated += (UpdateFileDataRequest file) =>
            {
                websocketConnectedController.SendMessageToUser(
                    file.UserID,
                    new WebSocketMessage(file)
                );
            };

            this._RemoveFileHandler = new RemoveFileDeviceOwnership();

            this._UpdateFileHandler = new UpdateIfOnlyOwnerChanged();
            _UpdateFileHandler.SetNext(new RenameIfOnlyPathChangedHandler());
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
                    FileUpdated.Invoke(
                        new UpdateFileDataRequest(UPDATE_TYPE.ADD, null, sync, user.id)
                    );
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
                SyncFileData fileInRepo = FileRepository.getNewestFileByPathNameExtensionAndUser(
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

        public void RemoveFile(FileData fileData, long ownerid, string deviceId)
        {
            SyncFileData file =
                this._RemoveFileHandler.Handle(
                    new RemoveFileDeviceOwnershipRequest()
                    {
                        deviceId = deviceId,
                        fileData = fileData,
                        userID = ownerid,
                    }
                ) as SyncFileData;
            if (file != null)
                this.FileUpdated(
                    new UpdateFileDataRequest(UPDATE_TYPE.DELETE, null, file, ownerid)
                );
        }

        public event FileUpdateHandler? FileUpdated;

        public void UpdateFileForDevice(
            string email,
            string deviceId,
            UpdateFileDataRequest fileUpdate
        )
        {
            fileUpdate.DeviceReuqested = deviceId;
            fileUpdate.UserID = UserRepository.getUserByMail(email).id;
            UpdateFileDataRequest resolved =
                _UpdateFileHandler.Handle(fileUpdate) as UpdateFileDataRequest;
            if (resolved != null)
            {
                this.FileUpdated(resolved);
            }
        }

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
