using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.SyncingHandlers;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Services
{
    class SyncFileService : IFileSyncService
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("SyncFileService");

        private IServerConnection _serverConnection;
        private IConfiguration _configuration;

        private IHandler _InitialSyncHandler;
        private ITaskRunController _taskRunController;
        private IFileRepositoryService _fileRepositoryService;

        public SyncFileService(
            IConfiguration configuration,
            IServerConnection serverConnection,
            IFileRepositoryService fileRepositoryService
        )
        {
            _configuration = configuration;
            _taskRunController = new RunningTaskController(configuration);
            _InitialSyncHandler = new GetLocalAndServerFileListHadndler(
                configuration,
                serverConnection,
                this
            );
            this._serverConnection = serverConnection;
            this._fileRepositoryService = fileRepositoryService;
            _taskRunController = new RunningTaskController(configuration);
            _InitialSyncHandler
                .SetNext(new DeleteCloudLocalFilesHandler())
                .SetNext(
                    new DownloadMissingFilesHandler(
                        configuration,
                        serverConnection,
                        _taskRunController,
                        fileRepositoryService
                    )
                )
                .SetNext(
                    new PerFileInitialSyncHandler(
                        configuration,
                        serverConnection,
                        _taskRunController,
                        fileRepositoryService
                    )
                );
            this._serverConnection.ConnectionChangeHandler += onConnnetionChange;
            this._serverConnection.ServerWerbsocketHadnler +=
                _serverConnection_ServerWerbsocketHadnler;
        }

        private void _serverConnection_ServerWerbsocketHadnler(WebSocketMessage message)
        {
            if (message.messageType == MESSAGE_TYPE.UPDATE)
            {
                onFileUPdate(message.data.syncFileData);
            }
        }

        private void onFileUPdate(SyncFileData syncFileData)
        {
            new PerFileInitialSyncHandler(
                this._configuration,
                this._serverConnection,
                _taskRunController,
                this._fileRepositoryService
            ).Handle(syncFileData);
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
            _InitialSyncHandler.Handle(null);
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

        private SyncState _state = SyncState.NOT_SETUP;

        public SyncState State
        {
            get { return _state; }
        }

        public void OnLocallyOnRenamed(RenamedEventArgs args)
        {
            logger.LogWarning($"OnLocallyOnRenamed Not Implemented:: {args.ToString()}");
        }

        public void OnLocallyDeleted(FileSystemEventArgs args)
        {
            logger.LogWarning($"OnLocallyDeleted Not Implemented:: {args.ToString()}");
        }

        public void OnLocallyCreated(FileSystemEventArgs args)
        {
            if (Active)
                _taskRunController.AddTask(
                    new UploadAction(
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService,
                        FileManager.GetUploadFileData(
                            args.FullPath,
                            this._configuration.StorageLocation
                        )
                    )
                );
        }

        public void OnLocallyChanged(FileSystemEventArgs args)
        {
            logger.LogWarning($"OnLocallyChanged Not Implemented:: {args.ToString()}");
        }

        public void StopAllSync()
        {
            logger.LogInformation("STOP all sync");
            _state = SyncState.STOPPED;
        }

        public void PauseAllSync()
        {
            _state = SyncState.PAUSED;
        }
    }
}
