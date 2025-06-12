using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;

namespace Cloud_Storage_Server.Handlers
{
    public class PrepareFileRemoveUpdateHandler : AbstactHandler
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator;
      public  PrepareFileRemoveUpdateHandler(IDataBaseContextGenerator dataBaseContextGenerator)
        {
            _dataBaseContextGenerator = dataBaseContextGenerator ?? throw new ArgumentNullException(nameof(dataBaseContextGenerator));
        }
        public override object Handle(object request)
        {
            RemoveFileDeviceOwnershipRequest removeFileDeviceOwnershipRequest = null;
            if (request is RemoveFileDeviceOwnershipRequest)
            {
                removeFileDeviceOwnershipRequest = request as RemoveFileDeviceOwnershipRequest;
            }

            if (removeFileDeviceOwnershipRequest is null)
            {
                throw new ArgumentException(
                    "PrepareFileRemoveUpdateHandler excepts argument of type RemoveFileDeviceOwnershipRequest"
                );
            }

            UpdateFileDataMessage updateFileDataMessage = new UpdateFileDataMessage(
                UPDATE_TYPE.DELETE,
                null,
                removeFileDeviceOwnershipRequest.syncFileData,
                removeFileDeviceOwnershipRequest.userID
            );

            updateFileDataMessage.DeviceReuqested = removeFileDeviceOwnershipRequest.deviceId;

            List<string> excludedDevices=new List<string>();
            excludedDevices.Add(removeFileDeviceOwnershipRequest.deviceId);
            using (var ctx= _dataBaseContextGenerator.GetDbContext())
            {
                excludedDevices = ctx.Files.ToList().Where(x =>
                        x.Id.Equals(removeFileDeviceOwnershipRequest.syncFileData.Id) && x.Hash.Length == 0)
                    .SelectMany(x => x.DeviceOwner).ToList();
            }

            UpdateFileDataMessageRequest updateFileDataMessageRequest = new UpdateFileDataMessageRequest()
            {
                updateFileDataMessage = updateFileDataMessage,
                UserIdToSendTo = removeFileDeviceOwnershipRequest.userID,
                InlcudedDevices = null,
                ExcludedDevices = excludedDevices
            };



            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(updateFileDataMessageRequest);
            }

            return updateFileDataMessage;
        }
    }
}
