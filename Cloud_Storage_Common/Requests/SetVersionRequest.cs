using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Common.Requests
{
    public class SetVersionRequest
    {
        public Guid FileId { get; set; }
        public ulong Version { get; set; }
    }
}
