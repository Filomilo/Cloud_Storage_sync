using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Cloud_Storage_Server.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Common.Models
{
    public class UploudFileData
    {
        [Required]
        [RegularExpression(
            $"{FileManager.RegexRelativePathValidation}",
            ErrorMessage = "Path string doesn't match path syntax"
        )]
        public string Path { get; set; }

        [Required]
        public string Name { get; set; }

        [NotNull]
        public string Extenstion { get; set; }

        [Required]
        public string Hash { get; set; }

        [Required]
        public DateTime SyncDate { get; set; }

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

    public class FileData : UploudFileData
    {
        private UploudFileData data;

        public FileData() { }

        public FileData(UploudFileData data)
        {
            this.Path = data.Path;
            this.SyncDate = data.SyncDate;
            this.Name = data.Name == null ? "" : data.Name;
            this.Hash = data.Hash;
            this.Extenstion = data.Extenstion == null ? "" : data.Extenstion;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [NotNull]
        [ForeignKey("User")]
        public long OwnerId { get; set; }
        public virtual User Owner { get; set; }
    }
}
