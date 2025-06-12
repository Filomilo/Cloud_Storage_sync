using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Services;

namespace Cloud_Storage_Server.Handlers
{




    public class SendUpdateToClientsHandler : AbstactHandler
    {
        IFileSyncService _fileSyncService;

        public SendUpdateToClientsHandler(IFileSyncService fileSyncService)
        {
            _fileSyncService = fileSyncService;
        }

        public override object Handle(object request)
        {
            UpdateFileDataMessageRequest update = null;
            if (request is UpdateFileDataMessageRequest)
            {
                UpdateFileDataMessageRequest fileUpdateMessage = (UpdateFileDataMessageRequest)request;
                update = fileUpdateMessage;
            }

            if (update is null)
            {
                throw new ArgumentException(
                    "SendUpdateToClientsHandler excepts argument of type UpdateFileDataMessageRequest"
                );
            }
            this._fileSyncService.SendFileUpdate(update);

            return request;
        }
    }
}
