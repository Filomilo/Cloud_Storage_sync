using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class DownloadMissingFilesHandler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IServerConnection _connection;
        private ITaskRunController _taskRunController;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger(
            "DownloadMissingFilesHandler"
        );

        public DownloadMissingFilesHandler(
            IConfiguration configuration,
            IServerConnection serverConnection,
            ITaskRunController taskRunController
        )
        {
            _configuration = configuration;
            _connection = serverConnection;
            _taskRunController = taskRunController;
        }

        private List<SyncFileData> getExclusieFileData(
            LocalAndServerFileData LocalAndServerFileData
        )
        {
            List<SyncFileData> filterd = LocalAndServerFileData
                .CloudFiles.Where(x =>
                    LocalAndServerFileData
                        .LocalFiles.Where(y => y.GetRealativePath().Equals(x.GetRealativePath()))
                        .Count() == 0
                )
                .ToList();
            return filterd;
            ;
        }

        public override object Handle(object request)
        {
            if (request.GetType() != typeof(LocalAndServerFileData))
            {
                throw new ArgumentException(
                    "DownloadMissingFilesHandler excepts argument of type LocalAndServerFileData"
                );
            }

            LocalAndServerFileData req = (LocalAndServerFileData)request;
            List<SyncFileData> filesToDownload = this.getExclusieFileData(req);
            foreach (SyncFileData syncFileData in filesToDownload)
            {
                _taskRunController.AddTask(
                    new DownloadAction(_connection, _configuration, syncFileData)
                );
            }

            if (this._nextHandler != null)
                this._nextHandler.Handle(request);
            return null;
        }
    }
}
