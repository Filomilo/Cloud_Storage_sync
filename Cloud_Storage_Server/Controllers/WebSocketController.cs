using System.Net.WebSockets;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Cloud_Storage_Server.Controllers
{
    public class WebSocketController : Controller
    {
        private readonly IWebsocketConnectedController websocketConnectedController;

        public WebSocketController(IWebsocketConnectedController websocketConnectedController)
        {
            this.websocketConnectedController = websocketConnectedController;
        }

        [SwaggerIgnore]
        [Route("/ws")]
        [Authorize]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Device device = DeviceRepository.GetDevice(
                    JwtHelpers.GetDeviceIDFromAuthString(Request.Headers.Authorization)
                );
                DeviceSocket deviceSocket = new DeviceSocket(device, webSocket);
                this.websocketConnectedController.AddDevice(deviceSocket);
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult res = await webSocket.ReceiveAsync(
                    ArraySegment<byte>.Empty,
                    CancellationToken.None
                );
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
