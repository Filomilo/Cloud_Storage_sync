using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;
using Microsoft.IdentityModel.Tokens;

namespace Cloud_Storage_Desktop_lib.Services
{
   public static class CredentialManager
    {
        private const string target="CloudDrive";
        public static void SaveToken(string token)
        {
            
            using (var cred = new Credential())
            {
                cred.Password = token;
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.LocalComputer;
                cred.Target=(target);
                cred.Save();
            }
        }

        public static string GetToken()
        {
            Credential cd = new Credential() { Target = target };
            cd.Load();
            return cd.Password;

        }

        public static void removeToken()
        {
            Credential cd = new Credential() { Target = target };
            cd.Load();
            cd.Delete();
        }
    }
}
