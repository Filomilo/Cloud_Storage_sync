using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    /// <summary>
    /// this handler works for multiple iefls dleted on server
    /// </summary>
    public class DeleteCloudLocalFilesHandler : AbstactHandler
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger(
            "DeleteCloudLocalFilesHandler"
        );

        public override object Handle(object request)
        {
            //if (request.GetType() != typeof(LocalAndServerFileData))
            //{
            //    throw new ArgumentException(
            //        "DeleteCloudLocalFilesHandler excepts argument of type LocalAndServerFileData"
            //    );
            //}

            //logger.LogWarning(
            //    "DeleteCloudLocalFilesHandler Not implemented, it should delete local files that were deleted on server"
            //);
            if (this._nextHandler != null)
                this._nextHandler.Handle(request);
            return null;
        }
    }
}
