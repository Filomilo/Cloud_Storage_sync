using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (request is not UpdateFileDataRequest)
                throw new ArgumentException(
                    "SendLocalFileUpdateToServer excepts argument of type UpdateFileDataRequest"
                );

            UpdateFileDataRequest updateFileDataRequest = request as UpdateFileDataRequest;

            this._serverConnection.UpdateFileData(updateFileDataRequest);

            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(updateFileDataRequest);
            }
            return updateFileDataRequest;
        }
    }
}
