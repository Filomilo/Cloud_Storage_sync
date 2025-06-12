using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    public class SendLocalFileUpdateToServer : AbstactHandler
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger(
            "UpdateDataBaseFileNameHandler"
        );
        IServerConnection _serverConnection;

        public SendLocalFileUpdateToServer(IServerConnection serverConnection)
        {
            _serverConnection = serverConnection;
        }

        public override object Handle(object request)
        {
            if (request is not UpdateFileDataMessage)
                throw new ArgumentException(
                    "SendLocalFileUpdateToServer excepts argument of type UpdateFileDataRequest"
                );

            UpdateFileDataMessage updateFileDataMessage = request as UpdateFileDataMessage;

            this._serverConnection.UpdateFileData(updateFileDataMessage);

            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(updateFileDataMessage);
            }
            return updateFileDataMessage;
        }
    }
}
