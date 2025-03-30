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
    class PrepareFileSyncData : AbstactHandler
    {
        private IConfiguration _configuration;

        public PrepareFileSyncData(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private ILogger logger = CloudDriveLogging.Instance.loggerFactory.CreateLogger(
            "PrepareFileSyncData"
        );

        public override object Handle(object request)
        {
            logger.LogDebug($"Begin PrepareFileSyncData for  {request.ToString()}");
            try
            {
                if (request.GetType() != typeof(String))
                    throw new ArgumentException("This handler exepts String");

                UploudFileData upcloudFileData = FileManager.GetUploadFileData(
                    request.ToString(),
                    _configuration.StorageLocation
                );

                if (this._nextHandler != null)
                    return this._nextHandler.Handle(upcloudFileData);
                else
                    return upcloudFileData;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error PrepareFileSyncData: {ex.Message}");
            }

            return null;
        }
    }
}
