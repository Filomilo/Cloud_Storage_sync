using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lombok.NET;
using Microsoft.OpenApi.Services;

namespace Cloud_Storage_Desktop_lib
{
    [ToString]
    public  partial class  Configuration
    {
        private const string _ApiUrl = "http://localhost:5087";

        public string ApiUrl
        {
            get
            {
                return _ApiUrl;
            }
        }

    }
}
