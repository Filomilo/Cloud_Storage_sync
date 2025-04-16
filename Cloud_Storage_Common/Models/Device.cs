using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Server.Database.Models;

namespace Cloud_Storage_Common.Models
{
    public class Device
    {
        [Key]
        public Guid Id { get; set; } = new Guid();

        [ForeignKey("user_device")]
        [Required]
        [NotNull]
        public long OwnerId { get; set; }
        public virtual User Owner { get; set; }
    }
}
