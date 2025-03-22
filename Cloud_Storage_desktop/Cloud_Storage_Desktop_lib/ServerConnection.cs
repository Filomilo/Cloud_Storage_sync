using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Services;
using log4net;
using log4net.Repository.Hierarchy;
using static Cloud_Storage_Server.Controllers.AuthController;

namespace Cloud_Storage_Desktop_lib
{
    public class ServerConnection
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
            AuthRequest auth = new AuthRequest()
            {
                Email = email,
                Password = password
            };
     

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
            AuthRequest auth = new AuthRequest()
            {
                Email = email,
                Password = password
            };


            HttpResponseMessage response = client.PostAsJsonAsync("/api/Auth/Register", auth).Result;
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
            string token =CredentialManager.GetToken();
            if (token.Length > 0)
            {
                try
                {
                    this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
        

    }
}
