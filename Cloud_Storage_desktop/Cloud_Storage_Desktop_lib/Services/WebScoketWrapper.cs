using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class WebSocketWrapper : IWebSocketWrapper
    {
        ClientWebSocket _clientWebSocket = new ClientWebSocket();

        public void Connect(Uri url, CancellationToken cancellationToken)
        {
            _clientWebSocket.ConnectAsync(url, cancellationToken).Wait();
        }

        public WebSocketReceiveResult ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken
        )
        {
            return _clientWebSocket.ReceiveAsync(buffer, cancellationToken).Result;
        }

        public WebSocketState State
        {
            get { return _clientWebSocket.State; }
        }

        public void Close(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken
        )
        {
            _clientWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken).Wait();
        }

        public void SetRequestHeader(string str1, string value)
        {
            this._clientWebSocket.Options.SetRequestHeader(str1, value);
        }
    }
}
