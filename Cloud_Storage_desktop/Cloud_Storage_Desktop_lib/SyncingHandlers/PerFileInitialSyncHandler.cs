using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class PerFileInitialSyncHandler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IServerConnection _connection;
        private IHandler _Handler;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("PerFileInitialSyncHandler");

        public PerFileInitialSyncHandler(
            IConfiguration configuration,
            IServerConnection serverConnection,
            ITaskRunController taskRunController,
            IFileRepositoryService fileRepositoryService
        )
        {
            _configuration = configuration;
            _connection = serverConnection;
            _Handler = new PrepareFileSyncData(configuration);
            _Handler
                .SetNext(new ValidateRenamedFileHandler(configuration, serverConnection))
                .SetNext(
                    new UploadNewFileHandler(
                        configuration,
                        serverConnection,
                        taskRunController,
                        fileRepositoryService
                    )
                );
        }

        public override object Handle(object request)
        {
            if (request.GetType() != typeof(LocalAndServerFileData))
            {
                throw new ArgumentException(
                    "PerFileInitialSyncHandler excepts argument of type LocalAndServerFileData"
                );
            }

            LocalAndServerFileData localAndServerFileData = (LocalAndServerFileData)request;
            foreach (FileData localFile in localAndServerFileData.LocalFiles)
            {
                this._Handler.Handle(
                    localFile.getFullFilePathForBasePath(this._configuration.StorageLocation)
                );
            }
            if (this._nextHandler != null)
                this._nextHandler.Handle(null);
            return null;
        }
    }
}
