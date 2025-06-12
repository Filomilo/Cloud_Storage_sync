using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    internal class ValidateFileAlreadyDeleted: AbstactHandler
    {
        private IConfiguration _configuration;

        public ValidateFileAlreadyDeleted(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public override object Handle(object request)
        {
            SyncFileData syncFileData = null;
            if (request is SyncFileData)
                syncFileData = request as SyncFileData;
            if (request is UpdateFileDataMessage)
                syncFileData = (request as UpdateFileDataMessage).newFileData;
            if (syncFileData == null)
                throw new ArgumentException(
                    "DeleteUpdateFileHandler excepts argument of type SyncFileData or UpdateFileDataRequest"
                );
            List<FileData> fileDatas = FileManager.GetAllFilesInLocation(
                this._configuration.StorageLocation
            );

           if( fileDatas.Where(x=>x.GetRealativePath().Equals(syncFileData.GetRealativePath())).Count()==0)
            {
                return request;
            }

            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(request);
            }

            return request;
        }
    }
}
