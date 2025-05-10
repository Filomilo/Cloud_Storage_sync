using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Common.Requests;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        private Task serverWatcherTask;
        private CancellationTokenSource cancellationTokenSourceServerWatcher;
        private bool _ServerStatus = false;

        private void CreateServerStatusWatcher()
        {
            cancellationTokenSourceServerWatcher = new CancellationTokenSource();
            serverWatcherTask = Task.Run(ServerWarcher, cancellationTokenSourceServerWatcher.Token);
        }

        ~ServerConnection()
        {
            if (cancellationTokenSourceServerWatcher != null)
            {
                cancellationTokenSourceServerWatcher.Cancel();
                cancellationTokenSourceServerWatcher.Dispose();
            }
        }

        private void ServerWarcher()
        {
            while (!cancellationTokenSourceServerWatcher.IsCancellationRequested)
            {
                Thread.Sleep(100 * 10);
                bool serverStatus = CheckIfHelathy();
                if (serverStatus != this._ServerStatus)
                {
                    _ServerStatus = serverStatus;
                    UpdateOnConncotionChange(_ServerStatus);
                }
            }
        }

        private void UpdateOnConncotionChange(bool state)
        {
            logger.LogTrace($"Conneciton change: {state}");
            if (ConnectionChangeHandler != null)
            {
                ConnectionChangeHandler.Invoke(state);
            }
        }

        public ServerConnection(
            string ConnetionAdress,
            ICredentialManager credentialManager,
            IWebSocketWrapper webSocketWrapper
        )
        {
            if (ConnetionAdress == "")
                return;
            try
            {
                CreateServerStatusWatcher();
                this._credentialManager = credentialManager;
                client.BaseAddress = new Uri(ConnetionAdress);
                this._webSocket = webSocketWrapper;
                this.AuthChangeHandler += UpdateWebsocketOnConnetionChange;
                this.ConnectionChangeHandler += LoadTokenOnConnectionChnage;
            }
            catch (Exception ex)
            {
                logger.LogError($"Coudlnt connect to server");
                if (this.ConnectionChangeHandler != null)
                {
                    this.ConnectionChangeHandler.Invoke(false);
                }
            }
        }

        private void LoadTokenOnConnectionChnage(bool state)
        {
            logger.LogTrace($"LoadTokenOnConnectionChnage: {state}");
            if (state)
            {
                _LoadToken();
            }
        }

        public ServerConnection(
            HttpClient client,
            ICredentialManager credentialManager,
            IWebSocketWrapper webSocketWrapper
        )
        {
            CreateServerStatusWatcher();
            this.client = client;
            this._credentialManager = credentialManager;
            this.AuthChangeHandler += UpdateWebsocketOnConnetionChange;
            this.ConnectionChangeHandler += LoadTokenOnConnectionChnage;

            this._webSocket = webSocketWrapper;
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
            try
            {
                HttpResponseMessage response = client.GetAsync("/api/Helath/health").Result;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    logger.LogError($"Cannot connect to server on url {this.client.BaseAddress}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                return false;
            }

            return true;
        }

        public bool CheckIfAuthirized()
        {
            HttpResponseMessage response;
            try
            {
                //logger.LogTrace(
                //    $"trying to get authorized server connction:: Credential magenr: {this._credentialManager.GetToken()} ---- server config :: {client.DefaultRequestHeaders}"
                //);
                response = client.GetAsync("/api/Helath/healthSecured").Result;
            }
            catch (Exception ex)
            {
                return false;
            }
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
                throw new UnauthorizedAccessException(
                    $"invalid login parameters::: {response.Content.ReadAsStringAsync().Result}"
                );
            }
            else
            {
                logger.LogInformation($"Succesfully regsitered with email {email}");
            }
            this._credentialManager.SaveToken(response.Content.ReadAsStringAsync().Result);
            _LoadToken();
        }

        private bool _authState = false;

        private void InovkeAuthChange(bool state)
        {
            if (_authState == state)
                return;
            _authState = state;
            logger.LogTrace($"Auth change: {state}");
            if (this.AuthChangeHandler != null)
            {
                this.AuthChangeHandler.Invoke(state);
            }
            else { }
        }

        private void _LoadToken()
        {
            string token = this._credentialManager.GetToken();
            logger.LogTrace($"_LoadToken :: {token}");
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
                        InovkeAuthChange(false);
                    }
                    else
                    {
                        InovkeAuthChange(true);
                    }
                }
                catch (Exception e)
                {
                    this._credentialManager.RemoveToken();
                    Console.WriteLine(e);
                }
            }
            else
            {
                InovkeAuthChange(false);
            }
        }

        public void Logout()
        {
            this.client.DefaultRequestHeaders.Authorization = null;
            this._credentialManager.RemoveToken();
            InovkeAuthChange(false);
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
                logger.LogError($"Failed to upload data [[{fileData}]]: {responseMesage}");
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
        public event OnAuthStateChange? AuthChangeHandler;
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

        public void SetFileVersion(Guid id, ulong version)
        {
            logger.LogDebug(
                $"SetFileVersion file on device {this._credentialManager.GetDeviceID()}"
            );
            SetVersionRequest setVersionRequest = new SetVersionRequest()
            {
                FileId = id,
                Version = version,
            };
            var response = this
                .client.PostAsJsonAsync("api/Files/setVersion", setVersionRequest)
                .Result;

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation($"File  data changed vereion successfully!");
            }
            else
            {
                string responseMesage = response.Content.ReadAsStringAsync().Result;
                logger.LogError($"Failed to set verstion file  data");
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
                throw new Exception(
                    $"Couldn't get File [[{guid.ToString()}]] form server: {response.Content.ReadAsStringAsync().Result}"
                );
            Stream stream = response.Content.ReadAsStream();
            return stream;
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public void AdressChange(string apiUrl)
        {
            if (this.client.BaseAddress != new Uri(apiUrl))
            {
                client.BaseAddress = new Uri(apiUrl);
            }
        }
    }
}
