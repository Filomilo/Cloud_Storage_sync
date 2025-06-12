using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class ValidateIfFileAlreadyExisitInDataBase : AbstactHandler
    {
        private ILogger Logger = CloudDriveLogging.Instance.GetLogger(
            "ValidateIfFileAlreadyExisitInDataBase"
        );
        IFileRepositoryService _fileRepositoryService;

        public ValidateIfFileAlreadyExisitInDataBase(IFileRepositoryService repository)
        {
            _fileRepositoryService = repository;
        }

        public override object Handle(object request)
        {
            Logger.LogInformation($"ValidateIfFileAlreadyExisitInDataBase:: [[{request}]]");
            LocalFileData syncFileData = null;
            if (request is SyncFileData)
                syncFileData = (LocalFileData)((SyncFileData)request);
            if (request is UploudFileData)
            {
                syncFileData = new LocalFileData((UploudFileData)request);
            }
            if (request is UpdateFileDataMessage)
                syncFileData = (LocalFileData)(request as UpdateFileDataMessage).newFileData;
            if (syncFileData == null)
                throw new ArgumentException(
                    "ValidateIfFileAlreadyExisitInDataBase excepts argument of type SyncFileData or UpdateFileDataRequest or UploudFileData"
                );
            bool doesEsist =
                _fileRepositoryService
                    .GetAllFiles()
                    .Where(x =>
                        x.GetRealativePath().Equals(syncFileData.GetRealativePath())
                        && x.Hash.Equals(syncFileData.Hash)
                    )
                    .Count() >= 1;
            if (doesEsist)
                return null;
            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(request);
            }
            return syncFileData;
        }
    }
}
