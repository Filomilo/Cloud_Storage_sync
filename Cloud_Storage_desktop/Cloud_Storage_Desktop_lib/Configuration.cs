using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lombok.NET;

namespace Cloud_Storage_Desktop_lib
{
    [ToString]
    public partial class Configuration
    {
        private const string _ApiUrl = "http://localhost:5087";

        public string ApiUrl
        {
            get { return _ApiUrl; }
        }
        private string _StorageLocation = "";

        //[RegularExpression(
        //    @"^@""^[a-zA-Z]:\\(?:[a-zA-Z0-9 _-]+\\)*[a-zA-Z0-9 _-]+\.txt$""",
        //    ErrorMessage = "Path string doesn't match path syntax"
        //)]
        public string StorageLocation
        {
            get { return _StorageLocation; }
            set { _StorageLocation = value; }
        }
    }
}
