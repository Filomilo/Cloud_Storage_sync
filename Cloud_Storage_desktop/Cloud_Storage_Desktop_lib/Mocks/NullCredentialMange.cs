using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Mocks
{
    internal class NullCredentialMange : ICredentialManager
    {
        public void SaveToken(string token) { }

        public string GetToken()
        {
            return "";
        }

        public void RemoveToken() { }

        public string GetDeviceID()
        {
            return "";
        }

        public string GetEmail()
        {
            return "";
        }
    }
}
