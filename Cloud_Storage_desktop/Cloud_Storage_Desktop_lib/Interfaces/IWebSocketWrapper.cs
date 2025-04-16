using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface IWebSocketWrapper
    {
        void Connect(Uri url, CancellationToken cancellationToken);
        WebSocketReceiveResult ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken
        );
        WebSocketState State { get; }

        void Close(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken
        );

        void SetRequestHeader(string str1, string value);
    }
}
