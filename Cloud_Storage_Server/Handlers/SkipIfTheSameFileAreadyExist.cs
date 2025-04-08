using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Services;

namespace Cloud_Storage_Server.Handlers
{
    public class SkipIfTheSameFileAlreadyExist : AbstactHandler
    {
        public override object Handle(object request)
        {
            if (request.GetType() != typeof(FileUploadRequest))
            {
                throw new ArgumentException(
                    "SkipIfTheSameFileAreadyExist excepts argument of type FileUploadRequest"
                );
            }

            FileUploadRequest fileUploadRequest = (FileUploadRequest)request;
            SyncFileData uploudFileData = fileUploadRequest.syncFileData;
            SyncFileData fileInRepositry = FileRepository.getFileByPathNameExtensionAndUser(
                uploudFileData.Path,
                uploudFileData.Name,
                uploudFileData.Extenstion,
                uploudFileData.OwnerId
            );
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
