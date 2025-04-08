using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Cloud_Storage_Server.Database.Models;
using Lombok.NET;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Common.Models
{
    public class FileData
    {
        [Required]
        [RegularExpression(
            $"{FileManager.RegexRelativePathValidation}",
            ErrorMessage = "Path string doesn't match path syntax"
        )]
        public virtual string Path { get; set; }

        [Required]
        public virtual string Name { get; set; }

        [NotNull]
        public virtual string Extenstion { get; set; }

        public override string ToString()
        {
            return $"{Path}{Name}{Extenstion}";
        }

        public string GetRealativePath()
        {
            return $"{this.Path}{this.Name}{this.Extenstion}";
        }

        public string GetFileNameANdExtenstion()
        {
            return $"{this.Name}{this.Extenstion}";
        }

        public string getFullPathForBasePath(string basepath)
        {
            return System.IO.Path.GetFullPath(this.Path, basepath);
        }

        public string getFullFilePathForBasePath(string basepath)
        {
            return System.IO.Path.GetFullPath(this.Path, basepath)
                + $"\\{this.Name}{this.Extenstion}";
        }
    }

    public class UploudFileData : FileData
    {
        [Required]
        [RegularExpression(
            $"{FileManager.RegexRelativePathValidation}",
            ErrorMessage = "Path string doesn't match path syntax"
        )]
        public override string Path { get; set; }

        [Required]
        public override string Name { get; set; }

        [NotNull]
        public override string Extenstion { get; set; }

        [Required]
        public string Hash { get; set; }

        [Required]
        public ulong Version { get; set; } = 0;
    }

    [PrimaryKey(nameof(Path), nameof(Name), nameof(Extenstion))]
    public class LocalFileData : UploudFileData
    {
        public LocalFileData() { }

        public LocalFileData(SyncFileData syncFileData)
        {
            this.Path = syncFileData.Path;
            this.Name = syncFileData.Name == null ? "" : syncFileData.Name;
            this.Hash = syncFileData.Hash;
            this.Extenstion = syncFileData.Extenstion == null ? "" : syncFileData.Extenstion;
            this.Version = syncFileData.Version;
        }

        public LocalFileData(UploudFileData syncFileData)
        {
            this.Path = syncFileData.Path;
            this.Name = syncFileData.Name == null ? "" : syncFileData.Name;
            this.Hash = syncFileData.Hash;
            this.Extenstion = syncFileData.Extenstion == null ? "" : syncFileData.Extenstion;
            this.Version = syncFileData.Version;
        }
    }

    [PrimaryKey(nameof(Id))]
    public class SyncFileData : UploudFileData
    {
        public SyncFileData() { }

        public SyncFileData(UploudFileData data)
        {
            this.Path = data.Path;
            this.Name = data.Name == null ? "" : data.Name;
            this.Hash = data.Hash;
            this.Extenstion = data.Extenstion == null ? "" : data.Extenstion;
        }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [NotNull]
        [ForeignKey("User")]
        public long OwnerId { get; set; }
        public virtual User Owner { get; set; }

        [Required]
        public DateTime SyncDate { get; set; }

        [ForeignKey("Devices")]
        public List<string> DeviceOwner { get; set; }
        public virtual List<Device> OwnersDevices { get; set; }
    }
}
