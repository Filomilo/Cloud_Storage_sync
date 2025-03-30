using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    public class DeleteCloudLocalFilesHandler : AbstactHandler
    {
        private ILogger logger = CloudDriveLogging.Instance.loggerFactory.CreateLogger(
            "DeleteCloudLocalFilesHandler"
        );

        public override object Handle(object request)
        {
            if (request.GetType() != typeof(LocalAndServerFileData))
            {
                throw new ArgumentException(
                    "DeleteCloudLocalFilesHandler excepts argument of type LocalAndServerFileData"
                );
            }

            logger.LogWarning(
                "DeleteCloudLocalFilesHandler Not implemented, it should delete local files that were deleted on server"
            );
            if (this._nextHandler != null)
                this._nextHandler.Handle(request);
            return null;
        }
    }
}
