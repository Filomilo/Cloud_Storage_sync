using System.Runtime.CompilerServices;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Services;
using Microsoft.EntityFrameworkCore;

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
            UpdateFileDataRequest update = null;
            if (request is UpdateFileDataRequest)
            {
                UpdateFileDataRequest fileUpdateRequest = (UpdateFileDataRequest)request;
                update = fileUpdateRequest;
            }

            if (update is null)
            {
                throw new ArgumentException(
                    "SendUpdateToClientsHandler excepts argument of type UpdateFileDataRequest"
                );
            }
            this._fileSyncService.SendFileUpdate(update);

            return request;
        }
    }
}
