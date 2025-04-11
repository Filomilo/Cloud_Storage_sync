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
    class DownloadNewFIleHandler : AbstactHandler
    {
        private ITaskRunController _taskRunController;
        private IServerConnection _serverConnection;
        private IConfiguration _configuration;
        private IFileRepositoryService _fileRepositoryService;

        public DownloadNewFIleHandler(
            ITaskRunController taskRunController,
            IServerConnection serverConnection,
            IConfiguration configuration,
            SyncFileData syncFileData,
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
            if (request.GetType() != typeof(SyncFileData))
                throw new ArgumentException(
                    "DownloadNewFIleHandler excepts argument of type SyncFileData"
                );
            SyncFileData syncFileData = (SyncFileData)request;
            _taskRunController.AddTask(
                new DownloadAction(
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
