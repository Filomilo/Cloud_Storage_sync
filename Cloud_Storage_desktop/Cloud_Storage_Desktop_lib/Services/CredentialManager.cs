using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class CredentialManager : ICredentialManager
    {
        private ILogger _logger = CloudDriveLogging.Instance.GetLogger("CredentialManager");

        public CredentialManager()
        {
            if (!File.Exists(getTokenFilePaath()))
            {
                SaveToken("");
            }
        }

        string getTokenFilePaath()
        {
            return Path.Combine(Cloud_Storage_Common.SharedData.GetAppDirectory(), "token");
        }

        public void SaveToken(string token)
        {
            using (var file = File.OpenWrite(getTokenFilePaath()))
            {
                StreamWriter writer = new StreamWriter(file);
                writer.Write(token);
                writer.Close();
            }
        }

        public string GetToken()
        {
            string token = "";
            using (var file = File.OpenRead(getTokenFilePaath()))
            {
                StreamReader writer = new StreamReader(file);
                token = writer.ReadToEnd();
            }
            //_logger.LogTrace($"Get token:: {token}");
            return token;
        }

        public void RemoveToken()
        {
            using (var file = File.OpenWrite(getTokenFilePaath()))
            {
                file.SetLength(0);
            }
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
