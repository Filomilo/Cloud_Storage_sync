using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface ICredentialManager
    {
        void SaveToken(string token);
        string GetToken();
        void RemoveToken();
    }
}
