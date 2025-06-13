using System.Timers;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;

namespace Cloud_Storage_Desktop_lib.Services
{
    class SyncFileService : IFileSyncService
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("SyncFileService");
        private static System.Timers.Timer _timer;

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

            this._serverConnection.AuthChangeHandler += onAuthChange;

            this._clientChainOfResponsibilityRepository = new ClientChainOfResponsibilityRepository(
                _taskRunController,
                _serverConnection,
                _fileRepositoryService,
                _configuration,
                this
            );

            _timer = new System.Timers.Timer(10000);
            _timer.AutoReset = true;
            _timer.Interval = 10000;
            _timer.Elapsed += runCyclicCallback;
            //_timer.Start();
        }

        private void runCyclicCallback(object? state, ElapsedEventArgs args)
        {
            _clientChainOfResponsibilityRepository.InitlalLocalySyncHandler.Handle(null);
        }

        private void _serverConnection_ServerWerbsocketHadnler(WebSocketMessage message)
        {
            if (message.messageType == MESSAGE_TYPE.UPDATE)
            {
                onFileUPdate(message.data.FlieUpdate);
            }
        }

        private void onFileUPdate(UpdateFileDataMessage syncFileData)
        {
            try
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
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"Error while handling file update for file [[{syncFileData.newFileData.GetRealativePath()}]] :::: {ex.Message}"
                );
            }
        }

        private void onAuthChange(bool state)
        {
            logger.LogInformation($"Connection For sync file serviece change to {state}");
            this._taskRunController.Active = state;
            if (state)
            {
                this._serverConnection.WebSocketConnection.ServerWerbsocketHadnler +=
                    _serverConnection_ServerWerbsocketHadnler;
                _clientChainOfResponsibilityRepository.InitlalConnectedSyncHandler.Handle(null);
            }

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
            _clientChainOfResponsibilityRepository.InitlalLocalySyncHandler.Handle(null);
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

        public void ResetSync()
        {
            logger.LogInformation("ResetSync");
            this._taskRunController.CancelAllTasks();
            this._fileRepositoryService.Reset();
        }

        public ITaskRunController runningTaskController
        {
            get { return this._taskRunController; }
        }

        public void OnLocallyOnRenamed(RenamedEventArgs args)
        {
            _clientChainOfResponsibilityRepository.OnLocalyFileRenamedHandler.Handle(args);
        }

        public void OnLocallyDeleted(FileSystemEventArgs args)
        {
            _clientChainOfResponsibilityRepository.OnLocalyFileDeletedHandler.Handle(args.FullPath);
        }

        public void OnLocallyChanged(FileSystemEventArgs args)
        {
            try
            {
                //SafeFileHandle handle = File.OpenHandle(args.FullPath);
                logger.LogDebug($"On Locally changed: [[{args.FullPath}]]");
                _clientChainOfResponsibilityRepository.OnLocallyFileChangeHandler.Handle(
                    args.FullPath
                );
                //handle.Close();
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"Error while handling file creation for file [[{args.Name}]] :::: {ex.Message}"
                );
            }
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
