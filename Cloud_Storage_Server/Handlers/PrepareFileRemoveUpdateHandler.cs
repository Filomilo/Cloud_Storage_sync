using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Server.Handlers
{
    public class PrepareFileRemoveUpdateHandler : AbstactHandler
    {
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

            UpdateFileDataRequest updateFileDataRequest = new UpdateFileDataRequest(
                UPDATE_TYPE.DELETE,
                null,
                removeFileDeviceOwnershipRequest.syncFileData,
                removeFileDeviceOwnershipRequest.userID
            );

            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(updateFileDataRequest);
            }

            return updateFileDataRequest;
        }
    }
}
