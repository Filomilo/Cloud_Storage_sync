using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Services;

namespace Cloud_Storage_Server.Handlers
{
    public class UpdateIfOnlyOwnerChanged : AbstactHandler
    {
        public override object Handle(object request)
        {
            SyncFileData uploudFileData = null;
            if (request is FileUploadRequest)
            {
                FileUploadRequest fileUploadRequest = (FileUploadRequest)request;
                uploudFileData = fileUploadRequest.syncFileData;
            }
            if (request is SyncFileData)
            {
                uploudFileData = request as SyncFileData;
            }

            if (request is UpdateFileDataRequest)
            {
                if ((request as UpdateFileDataRequest).newFileData == null)
                    throw new ArgumentException("File update quest hsould hav enew file data");
                uploudFileData = new SyncFileData((request as UpdateFileDataRequest).newFileData);
                uploudFileData.DeviceOwner = new List<string>()
                {
                    ((UpdateFileDataRequest)request).DeviceReuqested,
                };
                uploudFileData.OwnerId = ((UpdateFileDataRequest)request).UserID;
                if ((request as UpdateFileDataRequest).oldFileData != null)
                {
                    if (this._nextHandler != null)
                    {
                        return this._nextHandler.Handle(request);
                    }
                }
            }
            if (uploudFileData is null)
            {
                throw new ArgumentException(
                    "UpdateIfOnlyOwnerChanged excepts argument of type SyncFileData or FileUploadRequest or UpdateFileDataRequest"
                );
            }

            SyncFileData newestFileInRepository =
                FileRepository.getNewestFileByPathNameExtensionAndUser(
                    uploudFileData.Path,
                    uploudFileData.Name,
                    uploudFileData.Extenstion,
                    uploudFileData.OwnerId
                );

            if (
                newestFileInRepository != null
                && newestFileInRepository.Hash.Equals(uploudFileData.Hash)
                && !newestFileInRepository.DeviceOwner.Contains(uploudFileData.DeviceOwner.First())
            )
            {
                SyncFileData prevVersionOfFlieForThisDevice =
                    FileRepository.getFileByPathNameExtensionUserAndDeviceOwner(
                        uploudFileData.Path,
                        uploudFileData.Name,
                        uploudFileData.Extenstion,
                        uploudFileData.OwnerId,
                        uploudFileData.DeviceOwner.First()
                    );
                if (prevVersionOfFlieForThisDevice != null)
                {
                    SyncFileData prevVersionOfFlieForThisDeviceCopy = newestFileInRepository;
                    prevVersionOfFlieForThisDeviceCopy.DeviceOwner.Remove(
                        uploudFileData.DeviceOwner.First()
                    );
                    FileRepository.UpdateFile(
                        prevVersionOfFlieForThisDevice,
                        prevVersionOfFlieForThisDeviceCopy
                    );
                }

                SyncFileData copy = newestFileInRepository;
                copy.DeviceOwner.Add(uploudFileData.DeviceOwner.First());
                FileRepository.UpdateFile(newestFileInRepository, copy);
            }
            else
            {
                if (_nextHandler != null)
                    return this._nextHandler.Handle(request);
            }

            return FileRepository.getFileByPathNameExtensionAndUser(
                uploudFileData.Path,
                uploudFileData.Name,
                uploudFileData.Extenstion,
                uploudFileData.OwnerId
            );
        }
    }
}
