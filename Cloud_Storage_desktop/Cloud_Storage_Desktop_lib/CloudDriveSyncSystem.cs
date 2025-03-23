using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Lombok.NET;


namespace Cloud_Storage_Desktop_lib
{
    [Singleton]
    public partial class CloudDriveSyncSystem
    {
        private ServerConnection _ServerConnection;
        public ServerConnection ServerConnection
        {
            get { return _ServerConnection; }
        }

        private Configuration _Configuration=new Configuration();
        public Configuration Configuration
        {
            get { return _Configuration; }
        }

        private CloudDriveSyncSystem()
        {
            this._ServerConnection = new ServerConnection(this.Configuration.ApiUrl);
        }




    }
}
