using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class DeleteUpdateFileHandler : AbstactHandler
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("DeleteUpdateFileHandler");
        private ITaskRunController _taskRunController;
        private IServerConnection _serverConnection;
        private IConfiguration _configuration;
        private IFileRepositoryService _fileRepositoryService;

        public DeleteUpdateFileHandler(
            ITaskRunController taskRunController,
            IServerConnection serverConnection,
            IConfiguration configuration,
            IFileRepositoryService fileRepositoryService
        )
        {
            _taskRunController = taskRunController;
            _fileRepositoryService = fileRepositoryService;
            _serverConnection = serverConnection;
            _configuration = configuration;
        }

        public override object Handle(object request)
        {
            SyncFileData syncFileData = null;
            if (request is SyncFileData)
                syncFileData = request as SyncFileData;
            if (request is UpdateFileDataRequest)
                syncFileData = (request as UpdateFileDataRequest).newFileData;
            if (syncFileData == null)
                throw new ArgumentException(
                    "DeleteUpdateFileHandler excepts argument of type SyncFileData or UpdateFileDataRequest"
                );

            if (syncFileData.Hash != "")
            {
                logger.LogDebug($"File {syncFileData.GetRealativePath()} is not to delete.");
                if (this._nextHandler != null)
                    return this._nextHandler.Handle(request);
                return syncFileData;
            }
            _taskRunController.AddTask(
                new DeleteAction(
                    _serverConnection,
                    _configuration,
                    syncFileData,
                    _fileRepositoryService
                )
            );
            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(request);
            }
            return syncFileData;
        }
    }
}
