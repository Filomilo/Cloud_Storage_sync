using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lombok.NET;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Server.Database.Models
{
    [AllArgsConstructor]
    [With]
    [Index(nameof(User.mail), IsUnique = true)]
    public partial class  User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key,Column(Order = 0)]
        public long id { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [StringLength(50)]
        [RegularExpression(@"^[\w\.-]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Not valid email adress")]
        public string mail { get; set; }
        [Required]
        public string password { get; set; }
    }
}
