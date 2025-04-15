using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class RenameFileOnUpdateHandler : AbstactHandler
    {
        private ITaskRunController _taskRunController;
        private IServerConnection _serverConnection;
        private IConfiguration _configuration;
        private IFileRepositoryService _fileRepositoryService;

        public RenameFileOnUpdateHandler(
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
            UpdateFileDataRequest updateFileDataRequest = null;
            if (request is UpdateFileDataRequest)
                updateFileDataRequest = (request as UpdateFileDataRequest);
            if (updateFileDataRequest == null)
                throw new ArgumentException(
                    "DownloadNewFIleHandler excepts argument of type SyncFileData or UpdateFileDataRequest"
                );
            if (updateFileDataRequest.oldFileData == null)
            {
                if (this._nextHandler != null)
                    return this._nextHandler.Handle(request);
            }
            LocalFileData currentFileData = this._fileRepositoryService.GetFileByPathNameExtension(
                updateFileDataRequest.oldFileData.Path,
                updateFileDataRequest.oldFileData.Name,
                updateFileDataRequest.oldFileData.Extenstion
            );

            if (
                currentFileData == null
                || currentFileData.Version != updateFileDataRequest.oldFileData.Version
            )
            {
                if (this._nextHandler != null)
                    return this._nextHandler.Handle(request);
            }

            _taskRunController.AddTask(
                new RenameAction(
                    _serverConnection,
                    _configuration,
                    updateFileDataRequest,
                    _fileRepositoryService
                )
            );
            //if (this._nextHandler != null)
            //{
            //    return this._nextHandler.Handle(request);
            //}
            return updateFileDataRequest;
        }
    }
}
