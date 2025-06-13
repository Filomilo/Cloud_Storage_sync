using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface IWebSocketConnection
    {
        public WebSocketState State { get; }

        void AdressChange(string apiUrl);
        void Dispose();
        void EnsureWebSocketConnected();
        void SetAuthToken(string v);
        void CloseWebSocket();

        public event OnServerWebSockerMessage? ServerWerbsocketHadnler;
    }
}
