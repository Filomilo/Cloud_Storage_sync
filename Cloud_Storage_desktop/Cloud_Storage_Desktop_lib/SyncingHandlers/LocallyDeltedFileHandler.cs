using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class LocallyDeletedFileHandler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IServerConnection _serverConnection;

        public LocallyDeletedFileHandler(
            IConfiguration configuration,
            IServerConnection serverConnection
        )
        {
            this._configuration = configuration;
            this._serverConnection = serverConnection;
        }

        public override object Handle(object request)
        {
            if (request is not string)
            {
                throw new ArgumentException("LocallyDeletedFileHandler expects string as input");
            }

            string path = request as string;

            string relativePath = FileManager.GetRealtiveFullPathToFile(
                path,
                _configuration.StorageLocation
            );
            this._serverConnection.DeleteFile(relativePath);

            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(request);
            }

            return null;
        }
    }
}
