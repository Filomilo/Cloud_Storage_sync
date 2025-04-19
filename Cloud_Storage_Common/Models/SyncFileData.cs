using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Cloud_Storage_Server.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Common.Models
{
    public class FileData : IComparable
    {
        public FileData() { }

        public FileData(string relativePath)
        {
            FileManager.GetFilePathParamsFormRelativePath(
                relativePath,
                out string realitve,
                out string name,
                out string ext
            );
            this.Path = realitve;
            this.Name = name;
            this.Extenstion = ext;
        }

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

        public int CompareTo(object? obj)
        {
            if (obj is not FileData)
            {
                throw new ArgumentException("Object is not a FileData instance");
            }

            return this.Name.CompareTo((obj as FileData).Name);
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

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is null || this.GetType() != obj.GetType())
                return false;

            UploudFileData other = (UploudFileData)obj;

            return this.Path == other.Path
                && this.Name == other.Name
                && this.Extenstion == other.Extenstion
                && this.Hash == other.Hash
                && this.Version == other.Version;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Name, Extenstion, Hash, Version);
        }
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

        public static explicit operator LocalFileData(SyncFileData v)
        {
            return new LocalFileData()
            {
                Extenstion = v.Extenstion,
                Hash = v.Hash,
                Name = v.Name,
                Path = v.Path,
                Version = v.Version,
            };
        }

        public LocalFileData? Clone()
        {
            return new LocalFileData()
            {
                Extenstion = this.Extenstion,
                Hash = this.Hash,
                Name = this.Name,
                Path = this.Path,
                Version = this.Version,
            };
        }

        public override string ToString()
        {
            return $"Path: {this.Path}, Name: {this.Name}, Extension: {this.Extenstion}, Hash: {this.Hash}, Version: {this.Version}";
        }
    }

    [PrimaryKey(nameof(Id), nameof(Path), nameof(Name), nameof(Extenstion))]
    public class SyncFileData : UploudFileData
    {
        public SyncFileData() { }

        public SyncFileData(UploudFileData data)
        {
            this.Name = data.Name == null ? "" : data.Name;
            this.Hash = data.Hash;
            this.Extenstion = data.Extenstion == null ? "" : data.Extenstion;
            this.Path = data.Path;
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

        public override bool Equals(object? o)
        {
            if (ReferenceEquals(this, o))
                return true;
            if (this is null)
                return false;
            if (o is null)
                return false;
            if (o.GetType() != o.GetType())
                return false;
            SyncFileData obj = (SyncFileData)o;
            return this.Path == obj.Path
                && this.Name == obj.Name
                && this.Extenstion == obj.Extenstion
                && this.Hash == obj.Hash
                && this.Version == obj.Version
                && this.Id.Equals(obj.Id)
                && this.OwnerId == obj.OwnerId
                && this.SyncDate.Equals(obj.SyncDate)
                && Enumerable.SequenceEqual(this.DeviceOwner, obj.DeviceOwner);
        }

        public SyncFileData Clone()
        {
            return new SyncFileData
            {
                Path = this.Path,
                Name = this.Name,
                Extenstion = this.Extenstion,
                Hash = this.Hash,
                Version = this.Version,
                Id = this.Id,
                OwnerId = this.OwnerId,
                SyncDate = this.SyncDate,
                DeviceOwner = new List<string>(this.DeviceOwner),
            };
        }

        public override string ToString()
        {
            return base.ToString()
                + $"\n Version ::: {this.Version} \n  Hash ::: [[{this.Hash}]]\n Device owners:: [[{String.Join(", ", this.DeviceOwner)}]] \n Owner id: [[{this.OwnerId}]]  \n";
        }

        public SyncFileData(LocalFileData data, long OnwerID)
        {
            DeviceOwner = new List<string>();
            Extenstion = data.Extenstion;
            Hash = data.Hash;
            Name = data.Name;
            Path = data.Path;
            Version = data.Version;
            OwnerId = OnwerID;
            SyncDate = DateTime.Now;
        }
    }
}
