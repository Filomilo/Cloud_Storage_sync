﻿using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class UploadNewFileHandler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IServerConnection _connection;
        private ITaskRunController _taskRunController;
        private IFileRepositoryService _fileRepositoryService;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("UploadNewFileHandler");

        public UploadNewFileHandler(
            IConfiguration configuration,
            IServerConnection serverConnection,
            ITaskRunController taskRunController,
            IFileRepositoryService fileRepositoryService
        )
        {
            _configuration = configuration;
            _connection = serverConnection;
            _taskRunController = taskRunController;
            _fileRepositoryService = fileRepositoryService;
        }

        public override object Handle(object request)
        {
            if (request is not UploudFileData)
            {
                throw new ArgumentException(
                    "UploadNewFileHandler excepts argument of type LocalAndServerFileData"
                );
            }
            UploudFileData uploudFileData = (UploudFileData)request;
            _taskRunController.AddTask(
                new UploadAction(
                    this._connection,
                    this._configuration,
                    this._fileRepositoryService,
                    uploudFileData
                )
            );

            if (this._nextHandler != null)
                this._nextHandler.Handle(request);
            return null;
        }
    }
}
