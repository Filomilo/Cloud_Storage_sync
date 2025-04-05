using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using log4net;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cloud_Storage_Desktop_lib
{
    public class ServerConnection : IServerConnection
    {
        private static ILogger logger = CloudDriveLogging.Instance.GetLogger("ServerConnection");
        HttpClient client = new HttpClient();
        private ICredentialManager _credentialManager;

        public ServerConnection(string ConnetionAdress, ICredentialManager credentialManager)
        {
            this._credentialManager = credentialManager;
            client.BaseAddress = new Uri(ConnetionAdress);
            if (!CheckIfHelathy())
            {
                throw new ArgumentException("Server is not healthy");
            }
            _LoadToken();
        }

        public ServerConnection(HttpClient client, ICredentialManager credentialManager)
        {
            this.client = client;
            this._credentialManager = credentialManager;
            _LoadToken();
        }

        public bool CheckIfHelathy()
        {
            HttpResponseMessage response = client.GetAsync("/api/Helath/health").Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError($"Cannot connect to server on url {this.client.BaseAddress}");
                return false;
            }

            return true;
        }

        public bool CheckIfAuthirized()
        {
            HttpResponseMessage response = client.GetAsync("/api/Helath/healthSecured").Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError(
                    $"Cannot connect to AUTHorized server on url {this.client.BaseAddress}"
                );
                return false;
            }

            return true;
        }

        public void login(string email, string password)
        {
            AuthRequest auth = new AuthRequest() { Email = email, Password = password };

            HttpResponseMessage response = client.PostAsJsonAsync("/api/Auth/login", auth).Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Couldn't login for auth {email}");
                throw new UnauthorizedAccessException($"invalid login parameters");
            }
            this._credentialManager.SaveToken(response.Content.ReadAsStringAsync().Result);
            _LoadToken();
        }

        public void Register(string email, string password)
        {
            logger.LogInformation($"Registering with email {email}");
            AuthRequest auth = new AuthRequest() { Email = email, Password = password };

            HttpResponseMessage response = client
                .PostAsJsonAsync("/api/Auth/Register", auth)
                .Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Couldn't login for auth {email}");
                throw new UnauthorizedAccessException($"invalid login parameters");
            }
            this._credentialManager.SaveToken(response.Content.ReadAsStringAsync().Result);
            _LoadToken();
        }

        private void _LoadToken()
        {
            string token = this._credentialManager.GetToken();
            if (token.Length > 0)
            {
                try
                {
                    this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        token
                    );
                    if (!this.CheckIfAuthirized())
                    {
                        logger.LogWarning("Token authirzation failed");
                        this._credentialManager.RemoveToken();
                    }
                }
                catch (Exception e)
                {
                    this._credentialManager.RemoveToken();
                    Console.WriteLine(e);
                }
            }
        }

        public void Logout()
        {
            this.client.DefaultRequestHeaders.Authorization = null;
            this._credentialManager.RemoveToken();
        }

        public void UploudFile(UploudFileData fileData, Stream stream)
        {
            var form = FileMangamentSerivce.GetFormDatForFile(fileData, stream);
            var response = this.client.PostAsync("api/Files/upload", form).Result;

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    $"File {fileData.GetRealativePath()} data uploaded successfully!"
                );
            }
            else
            {
                logger.LogError($"Failed to upload data: {response.StatusCode}");
                throw new Exception($"{response.Content.ReadAsStringAsync().Result}");
            }
        }

        public List<SyncFileData> GetListOfFiles()
        {
            var response = this.client.GetAsync("api/Files/list").Result;
            var raw = response.Content.ReadAsStringAsync().Result;
            List<SyncFileData> parsed = JsonConvert.DeserializeObject<List<SyncFileData>>(raw);
            //var parsed = JsonSerializer.Deserialize<List<SyncFileData>>(raw);

            return parsed;
        }

        internal class FileDownloadRequest
        {
            public Guid guid { get; set; }
        }

        public List<SyncFileData> GetAllCloudFilesInfo()
        {
            var response = this.client.GetAsync("api/Files/list").Result;
            var raw = response.Content.ReadAsStringAsync().Result;
            List<SyncFileData> parsed = JsonConvert.DeserializeObject<List<SyncFileData>>(raw);
            //var parsed = JsonSerializer.Deserialize<List<SyncFileData>>(raw)
            return parsed;
        }

        public Stream DownloadFile(Guid guid)
        {
            var response = this
                .client.GetAsync($"api/Files/download?guid={guid.ToString()}")
                .Result;
            if (!response.IsSuccessStatusCode)
                throw new Exception("Couldn't get File form server");
            Stream stream = response.Content.ReadAsStream();
            return stream;
        }
    }
}
