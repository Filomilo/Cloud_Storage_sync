﻿using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;

namespace Cloud_Storage_Server.Handlers
{
    public class SkipIfTheSameFileAlreadyExist : AbstactHandler
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator;

        public SkipIfTheSameFileAlreadyExist(IDataBaseContextGenerator dataBaseContextGenerator)
        {
            this._dataBaseContextGenerator = dataBaseContextGenerator;
        }

        public override object Handle(object request)
        {
            if (request is not FileUploadRequest)
            {
                throw new ArgumentException(
                    "SkipIfTheSameFileAreadyExist excepts argument of type FileUploadRequest"
                );
            }

            FileUploadRequest fileUploadRequest = (FileUploadRequest)request;
            SyncFileData uploudFileData = fileUploadRequest.syncFileData;
            SyncFileData fileInRepositry;
            using (var context = _dataBaseContextGenerator.GetDbContext())
            {
                fileInRepositry = FileRepository.getNewestFileByPathNameExtensionAndUser(
                    context,
                    uploudFileData.Path,
                    uploudFileData.Name,
                    uploudFileData.Extenstion,
                    uploudFileData.OwnerId
                );
            }

            if (
                fileInRepositry != null
                && fileInRepositry.DeviceOwner.Contains(uploudFileData.DeviceOwner.First())
                && fileInRepositry.Hash.Equals(uploudFileData.Hash)
            )
                return fileInRepositry;

            if (_nextHandler != null)
            {
                return this._nextHandler.Handle(fileUploadRequest);
            }
            return fileInRepositry;
        }
    }
}
