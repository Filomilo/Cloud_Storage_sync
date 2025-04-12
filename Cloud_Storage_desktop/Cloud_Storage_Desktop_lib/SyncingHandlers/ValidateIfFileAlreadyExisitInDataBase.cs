using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Interfaces;
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
            if (request is not SyncFileData)
                throw new ArgumentException(
                    "ValidateIfFileAlreadyExisitInDataBase excepts argument of type SyncFileData"
                );
            SyncFileData syncFileData = request as SyncFileData;
            bool doesEsist =
                _fileRepositoryService
                    .GetAllFiles()
                    .Count(x =>
                        x.GetRealativePath().Equals(syncFileData.GetRealativePath())
                        && x.Hash.Equals(syncFileData.Hash)
                    ) == 1;
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
