using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;
using CredentialManagement;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class CredentialManager : ICredentialManager
    {
        private const string target = "CloudDrive";

        public void SaveToken(string token)
        {
            using (var cred = new Credential())
            {
                cred.Password = token;
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.LocalComputer;
                cred.Target = (target);
                cred.Save();
            }
        }

        public string GetToken()
        {
            Credential cd = new Credential() { Target = target };
            cd.Load();
            return cd.Password;
        }

        public void RemoveToken()
        {
            Credential cd = new Credential() { Target = target };
            cd.Load();
            cd.Delete();
        }

        public string GetDeviceID()
        {
            return JwtHelpers.GetDeviceIDFromToken(this.GetToken());
        }
    }
}
