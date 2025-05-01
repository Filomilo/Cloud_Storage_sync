using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_desktop.Logic
{
    public class NullWebSocket : IWebSocketWrapper
    {
        public void Connect(Uri url, CancellationToken cancellationToken) { }

        public WebSocketReceiveResult ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken
        )
        {
            return null;
        }

        public WebSocketState State { get; }

        public void Close(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken
        ) { }

        public void SetRequestHeader(string str1, string value) { }
    }
}
