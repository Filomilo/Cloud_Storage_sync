using Cloud_Storage_Common;
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

        void RemoveFile(FileData fileData, long id, string deviceId);

        event FileUpdateHandler FileUpdated;
        void SendFileUpdate(UpdateFileDataRequest update);
        void UpdateFileForDevice(string email, string deviceId, UpdateFileDataRequest file);
    }

    public class FileSyncService : IFileSyncService
    {
        private IFileSystemService _fileSystemService;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("FileSyncService");
        private IServerChainOfResposibiltyRepository _serverChainOfResposibiltyRepository;
        private IDataBaseContextGenerator _dataBaseContextGenerator;
        private IServerConfig _serverConfig;

        public FileSyncService(
            IFileSystemService fileSystemService,
            IWebsocketConnectedController websocketConnectedController,
            IDataBaseContextGenerator dataBaseContextGenerator,
            IServerConfig serverConfig
        )
        {
            _fileSystemService = fileSystemService;
            _dataBaseContextGenerator = dataBaseContextGenerator;
            _serverConfig = serverConfig;
            this.FileUpdated += (UpdateFileDataRequest file) =>
            {
                websocketConnectedController.SendMessageToUser(
                    file.UserID,
                    new WebSocketMessage(file)
                );
            };

            this._serverChainOfResposibiltyRepository = new ServerChainOfResposibiltyRepository(
                this._fileSystemService,
                this,
                this._dataBaseContextGenerator,
                this._serverConfig
            );
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
                SyncFileData sync = (SyncFileData)
                    _serverChainOfResposibiltyRepository.OnFileAddHandler.Handle(fileUploadRequest);
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

        public void RemoveFile(FileData fileData, long ownerid, string deviceId)
        {
            this._serverChainOfResposibiltyRepository.OnFileDeleteHandler.Handle(
                new RemoveFileDeviceOwnershipRequest()
                {
                    deviceId = deviceId,
                    fileData = fileData,
                    userID = ownerid,
                }
            );
            //if (file != null)
            //    this.FileUpdated(
            //        new UpdateFileDataRequest(UPDATE_TYPE.DELETE, null, file, ownerid)
            //    );
        }

        public event FileUpdateHandler? FileUpdated;

        public void SendFileUpdate(UpdateFileDataRequest update)
        {
            if (FileUpdated != null)
                FileUpdated.Invoke(update);
        }

        public void UpdateFileForDevice(
            string email,
            string deviceId,
            UpdateFileDataRequest fileUpdate
        )
        {
            using (var context = _dataBaseContextGenerator.GetDbContext())
            {
                fileUpdate.UserID = UserRepository.getUserByMail(context, email).id;
            }
            fileUpdate.DeviceReuqested = deviceId;
            UpdateFileDataRequest resolved =
                this._serverChainOfResposibiltyRepository.OnFileUpdateHandler.Handle(fileUpdate)
                as UpdateFileDataRequest;
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
            using (var context = _dataBaseContextGenerator.GetDbContext())
            {
                List<SyncFileData> files = FileRepository.GetAllUserFiles(context, user.id);
                return files;
            }
        }
    }
}
