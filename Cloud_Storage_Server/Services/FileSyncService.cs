using System.Diagnostics;
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

    public class UpdateFileDataMessageRequest()
    {
        public UpdateFileDataMessage updateFileDataMessage { get; set; }

        public long UserIdToSendTo { get; set; }
        public List<string> InlcudedDevices { get; set; } = new List<string>();
        public List<string> ExcludedDevices { get; set; } = new List<string>();
    }

    public delegate void FileUpdateHandler(UpdateFileDataMessageRequest uploudFile);

    public interface IFileSyncService
    {
        public void AddNewFile(User user, string deviceId, UploudFileData data, Stream file);
        public Stream DownloadFile(User user, SyncFileData data);
        public List<SyncFileData> ListFilesForUser(User user);

        void RemoveFile(FileData fileData, long id, string deviceId);

        event FileUpdateHandler FileUpdated;
        void SendFileUpdate(UpdateFileDataMessageRequest update);
        void UpdateFileForDevice(string email, string deviceId, UpdateFileDataMessage file);
        void SetFileVersion(long useiD, Guid fileId, ulong version);
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
            this.FileUpdated += (UpdateFileDataMessageRequest file) =>
            {
                websocketConnectedController.SendMessageToUser(
                    file.UserIdToSendTo,
                    new WebSocketMessage(file.updateFileDataMessage),
                    file.InlcudedDevices,
                    file.ExcludedDevices
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
            logger.LogDebug(
                $"AddNewFile for [[{user}]] with data [[{data}]] and stream [[{file.Length}]]"
            );
            SyncFileData fileData = new SyncFileData(data);
            fileData.OwnerId = user.id;
            fileData.DeviceOwner = new List<string>();
            fileData.DeviceOwner.Add(deviceId);

            FileUploadRequest fileUploadRequest = new FileUploadRequest(fileData, file);
            try
            {
                SyncFileData sync = (SyncFileData)
                    _serverChainOfResposibiltyRepository.OnFileAddChain.Handle(fileUploadRequest);
                if (FileUpdated != null)
                {
                    FileUpdated.Invoke(
                        new UpdateFileDataMessageRequest()
                        {
                            UserIdToSendTo = user.id,
                            updateFileDataMessage = new UpdateFileDataMessage(
                                UPDATE_TYPE.ADD,
                                null,
                                sync,
                                user.id
                            ),
                            InlcudedDevices = new List<string>(),
                            ExcludedDevices = new List<string>() { deviceId },
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"Error saving file to server: [[]{ex.Message}] \n {ex.StackTrace}"
                );
                throw ex;
            }
        }

        public void RemoveFile(FileData fileData, long ownerid, string deviceId)
        {
            this._serverChainOfResposibiltyRepository.OnFileDeleteChain.Handle(
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

        public void SendFileUpdate(UpdateFileDataMessageRequest update)
        {
            logger.LogTrace($"SendFileUpdate:: [[{update}]] \n [[{new StackTrace().ToString()}]]");
            if (FileUpdated != null)
                FileUpdated.Invoke(update);
        }

        public void UpdateFileForDevice(
            string email,
            string deviceId,
            UpdateFileDataMessage fileUpdate
        )
        {
            using (var context = _dataBaseContextGenerator.GetDbContext())
            {
                fileUpdate.UserID = UserRepository.getUserByMail(context, email).id;
            }
            fileUpdate.DeviceReuqested = deviceId;
            UpdateFileDataMessage resolved =
                this._serverChainOfResposibiltyRepository.OnFileUpdateChain.Handle(fileUpdate)
                as UpdateFileDataMessage;
        }

        private static string GetRealtivePathForFile(User user, SyncFileData data)
        {
            return Path.Combine($"{user.id}", $"{data.Id}");
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

        public void SetFileVersion(long userId, Guid fileId, ulong version)
        {
            UpdateNewestVersionRequest updateNewestVersionRequest = new UpdateNewestVersionRequest()
            {
                fileId = fileId.ToString(),
                fileVession = version,
                userID = userId,
            };
            SyncFileData resolved =
                this._serverChainOfResposibiltyRepository.ChangeNewestVersionChain.Handle(
                    updateNewestVersionRequest
                ) as SyncFileData;

            if (resolved != null)
            {
                this.FileUpdated(
                    new UpdateFileDataMessageRequest()
                    {
                        UserIdToSendTo = resolved.OwnerId,
                        updateFileDataMessage = new UpdateFileDataMessage(
                            UPDATE_TYPE.ADD,
                            null,
                            resolved,
                            resolved.OwnerId
                        ),
                        InlcudedDevices = new List<string>(),
                        ExcludedDevices = new List<string>(),
                    }
                );
            }
        }
    }
}
