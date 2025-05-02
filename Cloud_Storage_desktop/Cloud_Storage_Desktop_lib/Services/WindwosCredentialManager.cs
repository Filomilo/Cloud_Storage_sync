using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;
using CredentialManagement;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class WindwosManager : ICredentialManager
    {
        private ILogger _logger = CloudDriveLogging.Instance.GetLogger("CredentialManager");
        private const string target = "CloudDrive";

        public void SaveToken(string token)
        {
            _logger.LogTrace($"SaveToken :: {token}");
            using (var cred = new Credential())
            {
                cred.Password = token;
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.Enterprise;
                cred.Target = (target);
                cred.Save();
            }
        }

        public string GetToken()
        {
            Credential cd = new Credential() { Target = target };
            cd.Load();
            _logger.LogTrace($"GetToken:: {cd.Password} ");
            return cd.Password;
        }

        public void RemoveToken()
        {
            _logger.LogTrace($"RemoveToken");
            Credential cd = new Credential() { Target = target };
            cd.Load();
            cd.Delete();
        }

        public string GetDeviceID()
        {
            return JwtHelpers.GetDeviceIDFromToken(this.GetToken());
        }

        public string GetEmail()
        {
            return JwtHelpers.GetEmailFromToken(this.GetToken());
        }
    }
}
