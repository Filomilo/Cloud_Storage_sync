using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.SyncingHandlers;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Services
{
    class SyncFileService : IFileSyncService
    {
        private ILogger logger = CloudDriveLogging.Instance.loggerFactory.CreateLogger(
            "SyncFileService"
        );

        private IConfiguration _configuration;

        private IHandler _InitialSyncHandler;
        private ITaskRunController _taskRunController;

        public SyncFileService(IConfiguration configuration, IServerConnection serverConnection)
        {
            _configuration = configuration;
            _taskRunController = new RunningTaskController(configuration);
            _InitialSyncHandler = new GetLocalAndServerFileListHadndler(
                configuration,
                serverConnection
            );
            _InitialSyncHandler
                .SetNext(new DeleteCloudLocalFilesHandler())
                .SetNext(new DownloadMissingFilesHandler(configuration, serverConnection))
                .SetNext(
                    new PerFileInitialSyncHandler(
                        configuration,
                        serverConnection,
                        _taskRunController
                    )
                );
        }

        public void StartSync()
        {
            _InitialSyncHandler.Handle(null);
        }

        public IEnumerable<ISyncProcess> GetAllSyncProcesses()
        {
            throw new NotImplementedException();
        }

        public void OnLocallyOnRenamed(RenamedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnLocallyDeleted(FileSystemEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnLocallyCreated(FileSystemEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnLocallyChanged(FileSystemEventArgs args)
        {
            logger.LogWarning($"OnLocallyChanged Not Implemented:: {args.ToString()}");
        }

        public void StopAllSync()
        {
            logger.LogWarning("StopAllSync Not Implemented");
        }
    }
}
