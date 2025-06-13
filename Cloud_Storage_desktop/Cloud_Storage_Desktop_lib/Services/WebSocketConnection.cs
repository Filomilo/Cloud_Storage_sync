using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class WebSocketConnection : IWebSocketConnection
    {
        private static ILogger logger = CloudDriveLogging.Instance.GetLogger("WebSocketConnection");

        private IWebSocketWrapper webSocketWrapper;
        private CancellationTokenSource threadCancellationTokenSource;
        private CancellationTokenSource WebScoketCancellationTokenSource;
        private string baseAdress = "";
        private Task webSocketListeningTask;

        private string webSocketConntionAdress
        {
            get
            {
                return $"ws://{baseAdress.Replace("http://", "")}{(baseAdress.Last().Equals('/') ? "" : '/')}ws";
            }
        }

        private string token { get; set; } = "";

        public WebSocketConnection(IWebSocketWrapper webSocketWrapper)
        {
            this.webSocketWrapper = webSocketWrapper;

            StartWebSocketThread();
        }

        public WebSocketState State { get; }

        public void AdressChange(string apiUrl)
        {
            this.baseAdress = apiUrl;
            this.restartWebScoketLisitingThread();
        }

        public void Dispose()
        {
            CloseWebSocket();
        }

        public void EnsureWebSocketConnected()
        {
            if (webSocketListeningTask == null || webSocketListeningTask.IsCompleted)
            {
                StartWebSocketThread();
            }
        }

        public void SetAuthToken(string v)
        {
            this.token = v;
            this.restartWebScoketLisitingThread();
        }

        public void CloseWebSocket()
        {
            if (this.WebScoketCancellationTokenSource != null)
                this.WebScoketCancellationTokenSource.Cancel();
            if (this.threadCancellationTokenSource != null)
                this.threadCancellationTokenSource.Cancel();

            if (this.webSocketWrapper.State == WebSocketState.Open)
                this.webSocketWrapper.Close(
                    WebSocketCloseStatus.EndpointUnavailable,
                    "",
                    CancellationToken.None
                );
        }

        private void restartWebScoketLisitingThread()
        {
            CloseWebSocket();
            StartWebSocketThread();
        }

        private void WebSocketLisitingThread()
        {
            try
            {
                while (this.webSocketWrapper.State == WebSocketState.Open)
                {
                    try
                    {
                        byte[] buffer = new byte[4096];
                        WebSocketReceiveResult result = this.webSocketWrapper.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            CancellationToken.None
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
                            logger.LogError(
                                $"Unhandled AggregateException: [[ {String.Join(", \n", ex.InnerExceptions.Select(x => x.Message))} ]]"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            $"Unkwon Error reciving webscoket messages [[ {ex.Message}  ]] \n [[{ex.StackTrace}]]"
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

        private void StartWebSocketThread()
        {
            try
            {
                if (this.baseAdress.Length == 0)
                    throw new InvalidOperationException("WebSocket base address is not set.");
                if (token.Length == 0)
                    throw new InvalidOperationException("WebSocket token is not set.");

                threadCancellationTokenSource = new CancellationTokenSource();
                WebScoketCancellationTokenSource = new CancellationTokenSource();
                if (
                    webSocketWrapper.State == WebSocketState.Aborted
                    || webSocketWrapper.State == WebSocketState.Closed
                )
                    webSocketWrapper.Close(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                this.webSocketWrapper.SetRequestHeader("Authorization", $"Bearer {token}");

                this.webSocketWrapper.Connect(
                    new Uri(this.webSocketConntionAdress),
                    WebScoketCancellationTokenSource.Token
                );

                webSocketListeningTask = new Task(
                    WebSocketLisitingThread,
                    threadCancellationTokenSource.Token
                );
                webSocketListeningTask.Start();
                logger.LogError(
                    $"--------------------------------------------- WEB SOCKET CONNECTION STARTED"
                );
            }
            catch (AggregateException ex)
            {
                logger.LogError(
                    $"Failed to start web scoket connection: [[{String.Join(", \n", ex.InnerExceptions)}]]"
                );
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to start web scoket connection: [[{ex.Message}]]");
            }
        }

        public event OnServerWebSockerMessage? ServerWerbsocketHadnler;
    }
}
