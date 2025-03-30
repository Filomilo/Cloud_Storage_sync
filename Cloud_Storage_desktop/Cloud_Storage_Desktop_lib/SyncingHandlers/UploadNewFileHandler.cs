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
    class UploadNewFileHandler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IServerConnection _connection;
        private ITaskRunController _taskRunController;
        private ILogger logger = CloudDriveLogging.Instance.loggerFactory.CreateLogger(
            "UploadNewFileHandler"
        );

        public UploadNewFileHandler(
            IConfiguration configuration,
            IServerConnection serverConnection,
            ITaskRunController taskRunController
        )
        {
            _configuration = configuration;
            _connection = serverConnection;
            _taskRunController = taskRunController;
        }

        public override object Handle(object request)
        {
            if (request.GetType() != typeof(UploudFileData))
            {
                throw new ArgumentException(
                    "UploadNewFileHandler excepts argument of type LocalAndServerFileData"
                );
            }
            UploudFileData uploudFileData = (UploudFileData)request;
            _taskRunController.AddTask(
                new SyncTask(
                    uploudFileData.getFullFilePathForBasePath(_configuration.StorageLocation),
                    () =>
                    {
                        logger.LogInformation(
                            $"---------------- Start UPLOAD::: {uploudFileData.getFullFilePathForBasePath(_configuration.StorageLocation)}"
                        );
                        Thread.Sleep(500);
                        logger.LogInformation(
                            $"***************************s STOP UPLOAD::: {uploudFileData.getFullFilePathForBasePath(_configuration.StorageLocation)}"
                        );
                    }
                )
            );

            logger.LogWarning(
                "UploadNewFileHandler Not implemented, it should Uploud new File to cloud"
            );
            if (this._nextHandler != null)
                this._nextHandler.Handle(request);
            return null;
        }
    }
}
