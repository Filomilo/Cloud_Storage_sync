using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface IServerConnection
    {
        List<SyncFileData> GetAllCloudFilesInfo();
    }
}
