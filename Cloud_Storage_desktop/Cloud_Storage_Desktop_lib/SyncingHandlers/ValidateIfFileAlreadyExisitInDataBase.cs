﻿using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class ValidateIfFileAlreadyExisitInDataBase : AbstactHandler
    {
        IFileRepositoryService _fileRepositoryService;

        public ValidateIfFileAlreadyExisitInDataBase(IFileRepositoryService repository)
        {
            _fileRepositoryService = repository;
        }

        public override object Handle(object request)
        {
            LocalFileData syncFileData = null;
            if (request is SyncFileData)
                syncFileData = (LocalFileData)((SyncFileData)request);
            if (request is UploudFileData)
            {
                syncFileData = new LocalFileData((UploudFileData)request);
            }
            if (request is UpdateFileDataRequest)
                syncFileData = (LocalFileData)(request as UpdateFileDataRequest).newFileData;
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
