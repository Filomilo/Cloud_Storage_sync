using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Cloud_Storage_Server.Database.Models
{
    public class UploudFileData
    {
        [Required]
        [RegularExpression(@"^((/[a-zA-Z0-9-_]+)+|/)$", ErrorMessage = "Path string doesn't match path syntax")]
        public string Path { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Extenstion { get; set; }
        [Required]
        public string Hash { get; set; }
        [Required]
        public DateTime SyncDate { get; set; }
    }


    public class FileData: UploudFileData
    {
        [Key]
        public Guid Id { get; set; }
 

        [BindNever]
        [Required]
        [NotNull]
        [ForeignKey("User")] public long OwnerId { get; set; }
        public virtual User Owner { get; set; }

    }
}
