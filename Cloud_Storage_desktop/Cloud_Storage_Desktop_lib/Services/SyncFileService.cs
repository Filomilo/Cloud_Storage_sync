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

        private IHandler _InitialSyncHandler;
        private IHandler _OnFileUpdateHandler;
        private IHandler _OnFileCreatedHandler;
        private IHandler _OnFileDeletedHandler;

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
            _OnFileUpdateHandler = new ValidateIfFileAlreadyExisitInDataBase(fileRepositoryService);
            _OnFileUpdateHandler
                .SetNext(
                    new DownloadNewFIleHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .SetNext(
                    new DeleteUpdateFileHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                );

            this._serverConnection.ConnectionChangeHandler += onConnnetionChange;
            this._serverConnection.ServerWerbsocketHadnler +=
                _serverConnection_ServerWerbsocketHadnler;
            // File created handler
            this._OnFileCreatedHandler = new PrepareFileSyncData(this._configuration);
            _OnFileCreatedHandler
                .SetNext(
                    new AddFileToDataBaseHandler(this._configuration, this._fileRepositoryService)
                )
                .SetNext(
                    new UploadNewFileHandler(
                        this._configuration,
                        this._serverConnection,
                        this._taskRunController,
                        this._fileRepositoryService
                    )
                );
            // On Lcoaly dleted
            this._OnFileDeletedHandler = new LocallyDeletedFileHandler(
                this._configuration,
                this._serverConnection
            );
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
            this._OnFileUpdateHandler.Handle(syncFileData);
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
            this._OnFileDeletedHandler.Handle(args.FullPath);
        }

        public void OnLocallyCreated(FileSystemEventArgs args)
        {
            _OnFileCreatedHandler.Handle(args.FullPath);
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
