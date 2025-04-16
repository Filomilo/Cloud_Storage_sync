using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.SyncingHandlers;
using Cloud_Storage_Server.Handlers;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Services
{
    class SyncFileService : IFileSyncService
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("SyncFileService");

        private IServerConnection _serverConnection;
        private IConfiguration _configuration;
        private ITaskRunController _taskRunController;
        private IFileRepositoryService _fileRepositoryService;

        private IHandler _OnFileDeletedHandler;
        private IHandler _RenameFileHandler;

        private IClientChainOfResponsibilityRepository _clientChainOfResponsibilityRepository;

        public SyncFileService(
            IConfiguration configuration,
            IServerConnection serverConnection,
            IFileRepositoryService fileRepositoryService
        )
        {
            _configuration = configuration;
            _taskRunController = new RunningTaskController(configuration);

            this._serverConnection = serverConnection;
            this._fileRepositoryService = fileRepositoryService;
            _taskRunController = new RunningTaskController(configuration);

            this._serverConnection.ConnectionChangeHandler += onConnnetionChange;
            this._serverConnection.ServerWerbsocketHadnler +=
                _serverConnection_ServerWerbsocketHadnler;

            this._clientChainOfResponsibilityRepository = new ClientChainOfResponsibilityRepository(
                _taskRunController,
                _serverConnection,
                _fileRepositoryService,
                _configuration,
                this
            );
        }

        private void _serverConnection_ServerWerbsocketHadnler(WebSocketMessage message)
        {
            if (message.messageType == MESSAGE_TYPE.UPDATE)
            {
                onFileUPdate(message.data.FlieUpdate);
            }
        }

        private void onFileUPdate(UpdateFileDataRequest syncFileData)
        {
            switch (syncFileData.UpdateType)
            {
                case UPDATE_TYPE.RENAME:
                    _clientChainOfResponsibilityRepository.OnCloudFileRenamedHandler.Handle(
                        syncFileData
                    );
                    break;
                case UPDATE_TYPE.CONTNETS:
                    _clientChainOfResponsibilityRepository.OnCloudFileChangeHandler.Handle(
                        syncFileData
                    );
                    break;
                case UPDATE_TYPE.DELETE:
                    _clientChainOfResponsibilityRepository.OnCloudFileDeletedHandler.Handle(
                        syncFileData
                    );
                    break;
                case UPDATE_TYPE.ADD:
                    _clientChainOfResponsibilityRepository.OnCloudFileCreatedHandler.Handle(
                        syncFileData
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            //this._OnFileUpdateHandler.Handle(syncFileData);
        }

        private void onConnnetionChange(bool state)
        {
            logger.LogInformation($"Connection For sync file serviece change to {state}");
            if (state)
            {
                this._state = SyncState.CONNECTED;
                this.StartSync();
            }
            else
            {
                this._state = SyncState.DISCONNECTED;
                this.StopAllSync();
            }
        }

        public bool Active
        {
            get { return this.State == SyncState.CONNECTED; }
        }

        public void StartSync()
        {
            logger.LogInformation("Start Syncing");
            _clientChainOfResponsibilityRepository.InitlalSyncHandler.Handle(null);
        }

        public IEnumerable<ISyncProcess> GetAllSyncProcesses()
        {
            throw new NotImplementedException();
        }

        public void ResumeAllSync()
        {
            if (this._serverConnection.CheckIfAuthirized())
            {
                _state = SyncState.CONNECTED;
            }
            else
            {
                this._state = SyncState.DISCONNECTED;
            }
        }

        public void Dispose()
        {
            this.PauseAllSync();
            ;
        }

        private SyncState _state = SyncState.NOT_SETUP;

        public SyncState State
        {
            get { return _state; }
        }

        public void OnLocallyOnRenamed(RenamedEventArgs args)
        {
            _clientChainOfResponsibilityRepository.OnLocalyFileRenamedHandler.Handle(args);
            //_RenameFileHandler.Handle(args);
        }

        public void OnLocallyDeleted(FileSystemEventArgs args)
        {
            _clientChainOfResponsibilityRepository.OnLocalyFileDeletedHandler.Handle(args.FullPath);
            //this._OnFileDeletedHandler.Handle(args.FullPath);
        }

        public void OnLocallyCreated(FileSystemEventArgs args)
        {
            try
            {
                _clientChainOfResponsibilityRepository.OnLocalyFileCreatedHandler.Handle(
                    args.FullPath
                );
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Error while handling file creation for file [[{args.Name}]] :::: {ex.Message}"
                );
            }
            //_OnFileCreatedHandler.Handle(args.FullPath);
            //if (Active)
            //    _taskRunController.AddTask(
            //        new UploadAction(
            //            this._serverConnection,
            //            this._configuration,
            //            this._fileRepositoryService,
            //            FileManager.GetUploadFileData(
            //                args.FullPath,
            //                this._configuration.StorageLocation
            //            )
            //        )
            //    );
        }

        public void OnLocallyChanged(FileSystemEventArgs args)
        {
            //_clientChainOfResponsibilityRepository.OnLocallyFileChangeHandler.Handle(args);
            logger.LogWarning($"OnLocallyChanged Not Implemented:: {args.ToString()}");
        }

        public void StopAllSync()
        {
            logger.LogInformation("STOP all sync");
            this._taskRunController.CancelAllTasks();
            _state = SyncState.STOPPED;
        }

        public void PauseAllSync()
        {
            _state = SyncState.PAUSED;
        }
    }
}
