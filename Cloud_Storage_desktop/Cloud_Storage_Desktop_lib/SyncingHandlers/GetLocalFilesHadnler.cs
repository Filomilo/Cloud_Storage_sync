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
    internal class GetUploudFiles : AbstactHandler
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("GetLocalFilesHadnler");
        private IConfiguration _configuration;

        public GetUploudFiles(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public override object Handle(object request)
        {
            List<UploudFileData> uploadFileDatas = FileManager.GetUploadFileDataInLocation(
                this._configuration.StorageLocation
            );

            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(uploadFileDatas);
            }

            return uploadFileDatas;
        }
    }
}
