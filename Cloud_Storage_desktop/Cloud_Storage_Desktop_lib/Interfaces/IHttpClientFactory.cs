using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface IHttpClientFactory
    {
        HttpClient GetHttpClient();
    }
}
