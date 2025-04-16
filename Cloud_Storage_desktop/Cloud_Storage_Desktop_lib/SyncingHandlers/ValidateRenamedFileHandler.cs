using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class ValidateRenamedFileHandler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IServerConnection _connection;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("ValidateRenamedFileHandler");

        public ValidateRenamedFileHandler(
            IConfiguration configuration,
            IServerConnection serverConnection
        )
        {
            _configuration = configuration;
            _connection = serverConnection;
        }

        public override object Handle(object request)
        {
            if (request.GetType() != typeof(UploudFileData))
            {
                throw new ArgumentException(
                    "ValidateRenamedFileHandler excepts argument of type LocalAndServerFileData"
                );
            }

            logger.LogWarning(
                "ValidateRenamedFileHandler Not implemented, it should check if there isnt any hash like thta already in database"
            );
            if (this._nextHandler != null)
                this._nextHandler.Handle(request);
            return null;
        }
    }
}
