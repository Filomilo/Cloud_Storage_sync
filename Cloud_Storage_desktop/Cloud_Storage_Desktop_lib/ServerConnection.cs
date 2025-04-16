using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cloud_Storage_Desktop_lib
{
    public class ServerConnection : IServerConnection
    {
        private static ILogger logger = CloudDriveLogging.Instance.GetLogger("ServerConnection");
        HttpClient client = new HttpClient();
        private IWebSocketWrapper _webSocket;
        private Thread WsThread;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private ICredentialManager _credentialManager;

        public ServerConnection(
            string ConnetionAdress,
            ICredentialManager credentialManager,
            IWebSocketWrapper webSocketWrapper
        )
        {
            this._credentialManager = credentialManager;
            client.BaseAddress = new Uri(ConnetionAdress);
            this._webSocket = webSocketWrapper;
            if (!CheckIfHelathy())
            {
                throw new ArgumentException("Server is not healthy");
            }
            _LoadToken();
            this.ConnectionChangeHandler += UpdateWebsocketOnConnetionChange;
        }

        public ServerConnection(
            HttpClient client,
            ICredentialManager credentialManager,
            IWebSocketWrapper webSocketWrapper
        )
        {
            this.client = client;
            this._credentialManager = credentialManager;
            this.ConnectionChangeHandler += UpdateWebsocketOnConnetionChange;
            this._webSocket = webSocketWrapper;
            _LoadToken();
        }

        private void UpdateWebsocketOnConnetionChange(bool state)
        {
            if (state)
            {
                StartWebScoketLisitingThread();
            }
            else
            {
                StopWebSocket();
            }
        }

        private void StopWebSocket()
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                _webSocket.Close(
                    WebSocketCloseStatus.NormalClosure,
                    "Resetting",
                    CancellationToken.None
                );
            }
        }

        private async void StartWebScoketLisitingThread()
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                _webSocket.Close(
                    WebSocketCloseStatus.NormalClosure,
                    "Resetting",
                    CancellationToken.None
                );
            }
            this._cts = new CancellationTokenSource();
            string baseAdress = this.client.BaseAddress.ToString().Replace("http://", "");
            string uri = $"ws://{baseAdress}ws";
            string token = this._credentialManager.GetToken();
            if (token != null && token.Length > 0)
            {
                WsThread = new Thread(() => ConnectAndListen(uri, token));
                WsThread.Start();
            }
        }

        private void ConnectAndListen(string uri, string token)
        {
            try
            {
                _webSocket.SetRequestHeader("Authorization", $"Bearer {token}");
                _webSocket.Connect(new Uri(uri), _cts.Token);
                while (_webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        byte[] buffer = new byte[4096];
                        WebSocketReceiveResult result = _webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            _cts.Token
                        );
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        logger.LogInformation($"Received: {message}");
                        WebSocketMessage webSocketMessage =
                            JsonOperations.ObjectFromJSon<WebSocketMessage>(message);
                        if (this.ServerWerbsocketHadnler != null)
                        {
                            this.ServerWerbsocketHadnler.Invoke(webSocketMessage);
                        }
                    }
                    catch (WebSocketException ex)
                    {
                        logger.LogError(
                            $"WebSocketException Error reciving webscoket messages [[ {ex.Message}  ]]"
                        );
                    }
                    catch (ObjectDisposedException ex)
                    {
                        logger.LogTrace("Webscoket dispodees");
                        break;
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerExceptions.Any(e => e is IOException))
                        {
                            logger.LogError(
                                $"IOException occurred while receiving WebSocket messages: [[ {ex.Message} ]]"
                            );
                            break;
                        }
                        else
                        {
                            logger.LogError($"Unhandled AggregateException: [[ {ex.Message} ]]");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            $"Unkwon Error reciving webscoket messages [[ {ex.Message}  ]]"
                        );
                    }
                }
            }
            catch (ThreadInterruptedException ex)
            {
                logger.LogDebug($"Webscoket interrupted {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error webscoket conneiton :: {ex.Message}");
            }
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

        private void InovkeConnectionChange(bool state)
        {
            if (this.ConnectionChangeHandler != null)
            {
                this.ConnectionChangeHandler.Invoke(state);
            }
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
                    InovkeConnectionChange(true);
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
            InovkeConnectionChange(false);
        }

        public void UploudFile(UploudFileData fileData, Stream stream)
        {
            logger.LogDebug(
                $"Upldoing file  file from device {this._credentialManager.GetDeviceID()}"
            );
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
                string responseMesage = response.Content.ReadAsStringAsync().Result;
                logger.LogError($"Failed to upload data: {responseMesage}");
                throw new Exception($"{response.Content.ReadAsStringAsync().Result}");
            }
        }

        public void UpdateFileData(UpdateFileDataRequest file)
        {
            logger.LogDebug($"Updating file on device {this._credentialManager.GetDeviceID()}");
            var response = this.client.PostAsJsonAsync("api/Files/update", file).Result;

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    $"File {file.newFileData.GetRealativePath()} data updated successfully!"
                );
            }
            else
            {
                string responseMesage = response.Content.ReadAsStringAsync().Result;
                logger.LogError($"Failed to update file  data [[{file}]]: {responseMesage}");
                throw new Exception($"{response.Content.ReadAsStringAsync().Result}");
            }
        }

        public event OnConnectionStateChange? ConnectionChangeHandler;
        public event OnServerWebSockerMessage? ServerWerbsocketHadnler;

        public WebSocketState WebSocketState
        {
            get { return this._webSocket.State; }
        }

        public void DeleteFile(string relativePath)
        {
            var response = this
                .client.DeleteAsync($"api/Files/delete?relativePath={relativePath}")
                .Result;

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation($"File {relativePath} data updated successfully!");
            }
            else
            {
                string responseMesage = response.Content.ReadAsStringAsync().Result;
                logger.LogError(
                    $"Failed to update file  data [[{relativePath}]]: {responseMesage}"
                );
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

        internal void Dispose()
        {
            this.client.Dispose();
        }
    }
}
