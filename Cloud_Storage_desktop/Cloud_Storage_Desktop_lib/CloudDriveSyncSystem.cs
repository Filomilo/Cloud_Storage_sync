using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lombok.NET;
using Microsoft.IdentityModel.Tokens;

namespace Cloud_Storage_Desktop_lib
{
    [Singleton]
    public partial class CloudDriveSyncSystem
    {
       public ServerConnection ServerConnection
       {
           get { return _ServerConnection; }
       }
        private ServerConnection _ServerConnection;

        private CloudDriveSyncSystem()
        {
            this._ServerConnection = new ServerConnection(Configuration.ApiUrl);
        }




    }
}
