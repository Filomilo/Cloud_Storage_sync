using Lombok.NET;

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

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is null || this.GetType() != obj.GetType())
                return false;

            UpdateFileDataRequest other = (UpdateFileDataRequest)obj;

            return this.UserID == other.UserID
                && this.DeviceReuqested == other.DeviceReuqested
                && Equals(this.oldFileData, other.oldFileData)
                && Equals(this.newFileData, other.newFileData);
        }
    }
}
