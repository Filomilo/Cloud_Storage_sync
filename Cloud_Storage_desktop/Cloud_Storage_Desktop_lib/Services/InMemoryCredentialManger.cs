using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class InMemoryCredentialMangerL : ICredentialManager
    {
        private string _token = "";

        public void SaveToken(string token)
        {
            _token = token;
        }

        public string GetToken()
        {
            return _token;
        }

        public void RemoveToken()
        {
            _token = "";
        }

        public string GetDeviceID()
        {
            return JwtHelpers.GetDeviceIDFromAuthString(_token);
        }

        public string GetEmail()
        {
            return JwtHelpers.GetEmailFromToken(_token);
        }
    }
}
