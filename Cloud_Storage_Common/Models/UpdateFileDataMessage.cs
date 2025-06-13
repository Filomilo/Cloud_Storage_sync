using System.ComponentModel.DataAnnotations;
using Lombok.NET;

namespace Cloud_Storage_Common.Models
{
    public enum UPDATE_TYPE
    {
        RENAME,
        CONTNETS,
        DELETE,
        ADD,
    }

    [AllArgsConstructor]
    [NoArgsConstructor]
    public partial class UpdateFileDataMessage
    {
        public long UserID { get; set; }
        public string DeviceReuqested { get; set; }
        public SyncFileData oldFileData { get; set; }
        public SyncFileData newFileData { get; set; }

        [Required]
        public UPDATE_TYPE UpdateType { get; set; }

        public UpdateFileDataMessage(
            UPDATE_TYPE update,
            LocalFileData? oldData,
            LocalFileData newData
        )
        {
            this.UpdateType = update;
            if (oldData != null)
                this.oldFileData = new SyncFileData(oldData);
            if (newData != null)
                this.newFileData = new SyncFileData(newData);
        }

        public UpdateFileDataMessage(
            UPDATE_TYPE update,
            SyncFileData value,
            SyncFileData sync,
            long userId
        )
        {
            this.UpdateType = update;
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

            UpdateFileDataMessage other = (UpdateFileDataMessage)obj;

            return this.UserID == other.UserID
                && this.DeviceReuqested == other.DeviceReuqested
                && Equals(this.oldFileData, other.oldFileData)
                && Equals(this.newFileData, other.newFileData);
        }

        public override string ToString()
        {
            return $"UpdateFileDataRequest: "
                + $"UserID={UserID}, "
                + $"DeviceReuqested={DeviceReuqested}, "
                + $"UpdateType={UpdateType}, "
                + $"oldFileData=[{oldFileData?.ToString() ?? "null"}], "
                + $"newFileData=[{newFileData?.ToString() ?? "null"}]";
        }
    }
}
