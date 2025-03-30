using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;

namespace Cloud_Storage_Desktop_lib
{
    public class ServerConnection : IServerConnection
    {
        private static ILog logger = LogManager.GetLogger(typeof(ServerConnection));
        HttpClient client = new HttpClient();

        public ServerConnection(string ConnetionAdress)
        {
            client.BaseAddress = new Uri(ConnetionAdress);
            if (!CheckIfHelathy())
            {
                throw new ArgumentException("Server is not healthy");
            }
            _LoadToken();
        }

        public ServerConnection(HttpClient client)
        {
            this.client = client;
            _LoadToken();
        }

        public bool CheckIfHelathy()
        {
            HttpResponseMessage response = client.GetAsync("/api/Helath/health").Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.Error($"Cannot connect to server on url {this.client.BaseAddress}");
                return false;
            }

            return true;
        }

        public bool CheckIfAuthirized()
        {
            HttpResponseMessage response = client.GetAsync("/api/Helath/healthSecured").Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.Error($"Cannot connect to server on url {this.client.BaseAddress}");
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
                logger.Error($"Couldn't login for auth {email}");
                throw new UnauthorizedAccessException($"invalid login parameters");
            }
            CredentialManager.SaveToken(response.Content.ReadAsStringAsync().Result);
            _LoadToken();
        }

        public void Register(string email, string password)
        {
            AuthRequest auth = new AuthRequest() { Email = email, Password = password };

            HttpResponseMessage response = client
                .PostAsJsonAsync("/api/Auth/Register", auth)
                .Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.Error($"Couldn't login for auth {email}");
                throw new UnauthorizedAccessException($"invalid login parameters");
            }
            CredentialManager.SaveToken(response.Content.ReadAsStringAsync().Result);
            _LoadToken();
        }

        private void _LoadToken()
        {
            string token = CredentialManager.GetToken();
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
                        logger.Warn("Token authirzation failed");
                        CredentialManager.removeToken();
                    }
                }
                catch (Exception e)
                {
                    CredentialManager.removeToken();
                    Console.WriteLine(e);
                }
            }
        }

        public void Logout()
        {
            this.client.DefaultRequestHeaders.Authorization = null;
            CredentialManager.removeToken();
        }

        public void uploudFile(UploudFileData file, byte[] data)
        {
            var form = FileMangamentSerivce.GetFormDatForFile(file, data);
            var response = this.client.PostAsync("api/Files/upload", form).Result;

            if (response.IsSuccessStatusCode)
            {
                logger.Info("Student data uploaded successfully!");
            }
            else
            {
                logger.Error($"Failed to upload data: {response.StatusCode}");
                throw new Exception($"{response.Content.ReadAsStringAsync().Result}");
            }
        }

        public List<FileData> GetListOfFiles()
        {
            var response = this.client.GetAsync("api/Files/list").Result;
            var raw = response.Content.ReadAsStringAsync().Result;
            List<FileData> parsed = JsonConvert.DeserializeObject<List<FileData>>(raw);
            //var parsed = JsonSerializer.Deserialize<List<SyncFileData>>(raw);

            return parsed;
        }

        internal class FileDownloadRequest
        {
            public Guid guid { get; set; }
        }

        public byte[] DownloadFlie(Guid guid)
        {
            var response = this
                .client.GetAsync($"api/Files/download?guid={guid.ToString()}")
                .Result;
            if (!response.IsSuccessStatusCode)
                throw new Exception("Couldn't get File form server");
            //MemoryStream stream = new MemoryStream(0);
            //response.Content.CopyToAsync(stream).Wait();
            Stream stream = response.Content.ReadAsStream();
            byte[] buff = new byte[stream.Length];
            stream.Read(buff, 0, (int)stream.Length);
            return buff;
        }

        public List<SyncFileData> GetAllCloudFilesInfo()
        {
            var response = this.client.GetAsync("api/Files/list").Result;
            var raw = response.Content.ReadAsStringAsync().Result;
            List<SyncFileData> parsed = JsonConvert.DeserializeObject<List<SyncFileData>>(raw);
            //var parsed = JsonSerializer.Deserialize<List<SyncFileData>>(raw)
            return parsed;
        }
    }
}
