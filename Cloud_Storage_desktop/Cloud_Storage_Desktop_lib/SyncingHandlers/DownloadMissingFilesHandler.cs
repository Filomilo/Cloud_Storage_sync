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
    class DownloadMissingFilesHandler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IServerConnection _connection;
        private ILogger logger = CloudDriveLogging.Instance.loggerFactory.CreateLogger(
            "DownloadMissingFilesHandler"
        );

        public DownloadMissingFilesHandler(
            IConfiguration configuration,
            IServerConnection serverConnection
        )
        {
            _configuration = configuration;
            _connection = serverConnection;
        }

        public override object Handle(object request)
        {
            if (request.GetType() != typeof(LocalAndServerFileData))
            {
                throw new ArgumentException(
                    "DownloadMissingFilesHandler excepts argument of type LocalAndServerFileData"
                );
            }

            logger.LogWarning(
                "DownloadMissingFilesHandler Not implemented, it should delete local files that were deleted on server"
            );
            if (this._nextHandler != null)
                this._nextHandler.Handle(request);
            return null;
        }
    }
}
