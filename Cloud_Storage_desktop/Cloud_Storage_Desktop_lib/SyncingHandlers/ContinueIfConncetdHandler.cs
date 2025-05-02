using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    public class ContinueIfConncetdHandler : AbstactHandler
    {
        private IServerConnection serverConnection;

        public ContinueIfConncetdHandler(IServerConnection serverConnection)
        {
            this.serverConnection = serverConnection;
        }

        public override object Handle(object request)
        {
            if (serverConnection.CheckIfAuthirized())
            {
                if (this._nextHandler != null)
                {
                    return this._nextHandler.Handle(request);
                }
            }

            return request;
        }
    }
}
