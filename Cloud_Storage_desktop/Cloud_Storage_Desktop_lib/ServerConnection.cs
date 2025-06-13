using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Common.Requests;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_desktop.Logic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cloud_Storage_Desktop_lib
{
    public class ServerConnection : IServerConnection
    {
        private static int instanceCounter = 0;
        private static ILogger logger = CloudDriveLogging.Instance.GetLogger("ServerConnection");

        //HttpClient client = new HttpClient();
        //private IWebSocketWrapper _webSocket;
        //private Task WsThread;
        //private CancellationTokenSource _cts = new CancellationTokenSource();
        private ICredentialManager _credentialManager;
        private Task serverWatcherTask;
        private CancellationTokenSource cancellationTokenSourceServerWatcher;

        //private CancellationTokenSource cancellationTokenSourceWsThread =
        //    new CancellationTokenSource();
        private bool _ServerStatus = false;
        private SelfSetHttpClientFactory _httpClientFactory = new SelfSetHttpClientFactory(
            new HttpClient()
        );
        private IWebSocketConnection _webSocketConnection;

        public IWebSocketConnection WebSocketConnection
        {
            get { return _webSocketConnection; }
        }

        public event OnConnectionStateChange? ConnectionChangeHandler;
        public event OnAuthStateChange? AuthChangeHandler;

        ~ServerConnection()
        {
            if (cancellationTokenSourceServerWatcher != null)
            {
                cancellationTokenSourceServerWatcher.Cancel();
                cancellationTokenSourceServerWatcher.Dispose();
            }
        }

        //private void ensureWebSocketConnecetd()
        //{
        //    if (
        //        this.WebSocketState == WebSocketState.Aborted
        //        || this.WebSocketState == WebSocketState.None
        //    )
        //    {
        //        this._webSocket.Close(
        //            WebSocketCloseStatus.EndpointUnavailable,
        //            "",
        //            CancellationToken.None
        //        );
        //        this.StopWebScoketLisitingThread();
        //        this.StartWebScoketLisitingThread();
        //    }
        //}



        private void UpdateOnConncotionChange(bool state)
        {
            logger.LogTrace($"Conneciton change: {state}");
            if (state == false)
                InovkeAuthChange(state);
            if (ConnectionChangeHandler != null)
            {
                ConnectionChangeHandler.Invoke(state);
            }
        }

        #region Setup

        public ServerConnection(
            string ConnetionAdress,
            ICredentialManager credentialManager,
            IWebSocketWrapper webSocketWrapper
        )
        {
            if (instanceCounter > 0 && !(webSocketWrapper is NullWebSocket))
                throw new Exception("Cannnot crete new serve connectiopn instnace");
            ;
            instanceCounter++;

            if (ConnetionAdress == "")
                return;
            try
            {
                CreateServerStatusWatcher();
                this._credentialManager = credentialManager;
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(ConnetionAdress);
                _httpClientFactory.SetHttpClient(client);
                WebSocketSetup(webSocketWrapper, ConnetionAdress);
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

        public ServerConnection(
            HttpClient client,
            ICredentialManager credentialManager,
            IWebSocketWrapper webSocketWrapper
        )
        {
            if (instanceCounter > 0)
                throw new Exception("Cannnot crete new serve connectiopn instnace");
            ;
            instanceCounter++;
            CreateServerStatusWatcher();
            _httpClientFactory.SetHttpClient(client);

            this._credentialManager = credentialManager;
            WebSocketSetup(webSocketWrapper, client.BaseAddress.AbsoluteUri);
            this.ConnectionChangeHandler += LoadTokenOnConnectionChnage;
        }

        private void WebSocketSetup(IWebSocketWrapper webSocketWrapper, String url)
        {
            this._webSocketConnection = new WebSocketConnection(webSocketWrapper);
            _webSocketConnection.AdressChange(url);

            this.AuthChangeHandler += (state) =>
            {
                if (state)
                {
                    this._webSocketConnection.SetAuthToken(this._credentialManager.GetToken());
                }
                else
                {
                    this._webSocketConnection.CloseWebSocket();
                }
            };
        }

        #endregion





        //private void UpdateWebsocketOnConnetionChange(bool state)
        //{
        //    if (state)
        //    {
        //        StartWebScoketLisitingThread();
        //    }
        //    else
        //    {
        //        StopWebScoketLisitingThread();
        //    }
        //}

        //private void StopWebSocket()
        //{
        //    if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        //    {
        //        _webSocket.Close(
        //            WebSocketCloseStatus.EndpointUnavailable,
        //            "Resetting",
        //            CancellationToken.None
        //        );
        //    }
        //}

        //private void StopWebScoketLisitingThread()
        //{
        //    cancellationTokenSourceWsThread.Cancel();

        //    WsThread.Wait();
        //    WsThread.Dispose();
        //}

        //private async void StartWebScoketLisitingThread()
        //{
        //    cancellationTokenSourceWsThread = new CancellationTokenSource();
        //    if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        //    {
        //        _webSocket.Close(
        //            WebSocketCloseStatus.NormalClosure,
        //            "Resetting",
        //            CancellationToken.None
        //        );
        //    }
        //    this._cts = new CancellationTokenSource();
        //    string baseAdress = _httpClientFactory
        //        .GetHttpClient()
        //        .BaseAddress.ToString()
        //        .Replace("http://", "");
        //    string uri = $"ws://{baseAdress}ws";
        //    string token = this._credentialManager.GetToken();
        //    if (token != null && token.Length > 0)
        //    {
        //        WsThread = null;
        //        WsThread = new Task(
        //            () => ConnectAndListen(uri, token),
        //            cancellationTokenSourceWsThread.Token
        //        );
        //        WsThread.Start();
        //    }
        //}

        //private void ConnectAndListen(string uri, string token)
        //{
        //    try
        //    {
        //        if (_webSocket != null)
        //        {
        //            _webSocket.Close(
        //                WebSocketCloseStatus.NormalClosure,
        //                "",
        //                CancellationToken.None
        //            );
        //            _webSocket = null;
        //        }
        //        _webSocket = new WebSocketWrapper();
        //        _webSocket.SetRequestHeader("Authorization", $"Bearer {token}");
        //        _webSocket.Connect(new Uri(uri), _cts.Token);
        //        while (_webSocket.State == WebSocketState.Open)
        //        {
        //            try
        //            {
        //                byte[] buffer = new byte[4096];
        //                WebSocketReceiveResult result = _webSocket.ReceiveAsync(
        //                    new ArraySegment<byte>(buffer),
        //                    _cts.Token
        //                );
        //                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        //                logger.LogInformation($"Received: {message}");
        //                WebSocketMessage webSocketMessage =
        //                    JsonOperations.ObjectFromJSon<WebSocketMessage>(message);
        //                if (this.ServerWerbsocketHadnler != null)
        //                {
        //                    Task.Run(() => this.ServerWerbsocketHadnler.Invoke(webSocketMessage));
        //                }
        //            }
        //            catch (WebSocketException ex)
        //            {
        //                logger.LogError(
        //                    $"WebSocketException Error reciving webscoket messages [[ {ex.Message}  ]]"
        //                );
        //            }
        //            catch (ObjectDisposedException ex)
        //            {
        //                logger.LogTrace("Webscoket dispodees");
        //                break;
        //            }
        //            catch (AggregateException ex)
        //            {
        //                if (ex.InnerExceptions.Any(e => e is IOException))
        //                {
        //                    logger.LogError(
        //                        $"IOException occurred while receiving WebSocket messages: [[ {ex.Message} ]]"
        //                    );
        //                    break;
        //                }
        //                else
        //                {
        //                    logger.LogError(
        //                        $"Unhandled AggregateException: [[ {String.Join(", \n", ex.InnerExceptions.Select(x => x.Message))} ]]"
        //                    );
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.LogError(
        //                    $"Unkwon Error reciving webscoket messages [[ {ex.Message}  ]]"
        //                );
        //            }
        //        }
        //    }
        //    catch (ThreadInterruptedException ex)
        //    {
        //        logger.LogDebug($"Webscoket interrupted {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Error webscoket conneiton :: {ex.Message}");
        //    }
        //}

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

        //public WebSocketState WebSocketState
        //{
        //    get { return this._webSocket == null ? WebSocketState.None : this._webSocket.State; }
        //}

        #region Server Status watching


        private void LoadTokenOnConnectionChnage(bool state)
        {
            logger.LogTrace($"LoadTokenOnConnectionChnage: {state}");
            if (state)
            {
                _LoadToken();
            }
        }

        private void CreateServerStatusWatcher()
        {
            if (serverWatcherTask != null)
            {
                DisposeConnectionStatusWatch();
            }

            cancellationTokenSourceServerWatcher = new CancellationTokenSource();
            serverWatcherTask = Task.Run(ServerWarcher, cancellationTokenSourceServerWatcher.Token);
        }

        private void ServerWarcher()
        {
            while (!cancellationTokenSourceServerWatcher.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(100 * 10);
                    bool serverStatus = CheckIfHelathy();
                    if (serverStatus)
                    {
                        bool authorized = CheckIfAuthirized();
                        if (authorized)
                        {
                            _webSocketConnection.EnsureWebSocketConnected();
                        }
                    }
                    if (serverStatus != this._ServerStatus)
                    {
                        _ServerStatus = serverStatus;
                        UpdateOnConncotionChange(serverStatus);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"ServerWarcher:: {ex.Message}");
                }
            }
        }

        #endregion

        #region ENDPOINTS
        public void UploudFile(UploudFileData fileData, Stream stream)
        {
            logger.LogDebug(
                $"Upldoing file  file from device {this._credentialManager.GetDeviceID()}"
            );
            var form = FileMangamentSerivce.GetFormDatForFile(fileData, stream);
            var response = _httpClientFactory
                .GetHttpClient()
                .PostAsync("api/Files/upload", form)
                .Result;

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

        public void UpdateFileData(UpdateFileDataMessage file)
        {
            logger.LogDebug($"Updating file on device {this._credentialManager.GetDeviceID()}");
            var response = _httpClientFactory
                .GetHttpClient()
                .PostAsJsonAsync("api/Files/update", file)
                .Result;

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

        public void Logout()
        {
            _httpClientFactory.GetHttpClient().DefaultRequestHeaders.Authorization = null;
            this._credentialManager.RemoveToken();
            InovkeAuthChange(false);
        }

        public bool CheckIfHelathy()
        {
            try
            {
                HttpResponseMessage response = _httpClientFactory
                    .GetHttpClient()
                    .GetAsync("/api/Helath/health")
                    .WaitAsync(new TimeSpan(0, 0, 0, 5))
                    .Result;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    logger.LogTrace(
                        $"Cannot connect to server on url {_httpClientFactory.GetHttpClient().BaseAddress}"
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                //logger.LogWarning(ex.Message);
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
                response = _httpClientFactory
                    .GetHttpClient()
                    .GetAsync("/api/Helath/healthSecured")
                    .WaitAsync(new TimeSpan(0, 0, 0, 100))
                    .Result;
            }
            catch (Exception ex)
            {
                return false;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                //logger.LogError(
                //    $"Cannot connect to AUTHorized server on url {_httpClientFactory.GetHttpClient().BaseAddress}"
                //);
                return false;
            }

            return true;
        }

        public void login(string email, string password)
        {
            AuthRequest auth = new AuthRequest() { Email = email, Password = password };

            HttpResponseMessage response = _httpClientFactory
                .GetHttpClient()
                .PostAsJsonAsync("/api/Auth/login", auth)
                .Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Couldn't login for auth {email}");
                throw new UnauthorizedAccessException($"invalid login parameters");
            }
            String token = response.Content.ReadAsStringAsync().Result;
            this._credentialManager.SaveToken(token);

            _LoadToken();
        }

        public void Register(string email, string password)
        {
            logger.LogInformation($"Registering with email {email}");
            AuthRequest auth = new AuthRequest() { Email = email, Password = password };

            HttpResponseMessage response = _httpClientFactory
                .GetHttpClient()
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

        public void DeleteFile(string relativePath)
        {
            var response = this
                ._httpClientFactory.GetHttpClient()
                .DeleteAsync($"api/Files/delete?relativePath={relativePath}")
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
                ._httpClientFactory.GetHttpClient()
                .PostAsJsonAsync("api/Files/setVersion", setVersionRequest)
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
            try
            {
                var response = this
                    ._httpClientFactory.GetHttpClient()
                    .GetAsync("api/Files/list")
                    .Result;
                var raw = response.Content.ReadAsStringAsync().Result;
                List<SyncFileData> parsed = JsonConvert.DeserializeObject<List<SyncFileData>>(raw);
                return parsed;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"GetListOfFiles:: {ex.Message}");
                return new List<SyncFileData>();
            }

            //var parsed = JsonSerializer.Deserialize<List<SyncFileData>>(raw);
        }

        public List<SyncFileData> GetAllCloudFilesInfo()
        {
            var response = this
                ._httpClientFactory.GetHttpClient()
                .GetAsync("api/Files/list")
                .Result;
            var raw = response.Content.ReadAsStringAsync().Result;
            List<SyncFileData> parsed = JsonConvert.DeserializeObject<List<SyncFileData>>(raw);
            //var parsed = JsonSerializer.Deserialize<List<SyncFileData>>(raw)
            return parsed;
        }

        public Stream DownloadFile(String path)
        {
            var response = this
                ._httpClientFactory.GetHttpClient()
                .GetAsync(
                    $"api/Files/download?path={path}",
                    HttpCompletionOption.ResponseHeadersRead
                )
                .Result;
            if (!response.IsSuccessStatusCode)
                throw new Exception(
                    $"Couldn't get File [[{path}]] form server: {response.Content.ReadAsStringAsync().Result}"
                );
            Stream stream = response.Content.ReadAsStream();
            return stream;
        }

        #endregion

        #region Configuring

        public void AdressChange(string apiUrl)
        {
            if (_httpClientFactory.GetHttpClient().BaseAddress != new Uri(apiUrl))
            {
                HttpClient httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(apiUrl);
                _httpClientFactory.SetHttpClient(httpClient);
                if (!CheckIfHelathy())
                {
                    logger.LogError($"Cannot connect to {apiUrl}");
                }
                this._webSocketConnection.AdressChange(apiUrl);
            }
        }

        private void _LoadToken()
        {
            string token = this._credentialManager.GetToken();
            logger.LogTrace($"_LoadToken :: {token}");
            if (token.Length > 0)
            {
                try
                {
                    _httpClientFactory.GetHttpClient().DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
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

        #endregion



        //internal class FileDownloadRequest
        //{
        //    public Guid guid { get; set; }
        //}

        private void DisposeConnectionStatusWatch()
        {
            this.cancellationTokenSourceServerWatcher.Cancel();
            this.serverWatcherTask.Wait(5000);
        }

        public void Dispose()
        {
            DisposeConnectionStatusWatch();
            this._webSocketConnection.Dispose();
            //this._webSocket.Close(WebSocketCloseStatus.Empty, "close", new CancellationToken());
            //this.WsThread.Wait(5000);
            this._httpClientFactory.GetHttpClient().Dispose();
        }
    }
}
