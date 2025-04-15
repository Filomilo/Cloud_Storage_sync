using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lombok.NET;
using Newtonsoft.Json;

namespace Cloud_Storage_Common.Models
{
    [AllArgsConstructor]
    [NoArgsConstructor]
    public partial class UpdateFileDataRequest
    {
        public long UserID { get; set; }
        public string DeviceReuqested { get; set; }
        public SyncFileData oldFileData { get; set; }
        public SyncFileData newFileData { get; set; }

        public UpdateFileDataRequest(LocalFileData? oldData, LocalFileData newData)
        {
            if (oldData != null)
                this.oldFileData = new SyncFileData(oldData);
            if (newData != null)
                this.newFileData = new SyncFileData(newData);
        }

        public UpdateFileDataRequest(SyncFileData value, SyncFileData sync, long userId)
        {
            this.UserID = userId;
            this.oldFileData = value;
            this.newFileData = sync;
        }
    }
}
