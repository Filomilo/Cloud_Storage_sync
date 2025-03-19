using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net.WebSockets;
using System.Text;

namespace Cloud_Storage_Server.Controllers
{
    
    public class WebSocketController : Controller
    {
        private readonly IWebsockerConnectionService websockerConnectionService;
        public WebSocketController(IWebsockerConnectionService websockerConnectionService)
        {
            this.websockerConnectionService = websockerConnectionService;
        }

        [SwaggerIgnore]
        [Route("/ws")]
        [Authorize]
        public async Task Get()
        {


            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                User connectedUser = UserRepository.getUserByMail(HttpContext.User.Identity.Name);
                
                this.websockerConnectionService.addConnetedUser(webSocket, connectedUser);

                var Buffrer = new Byte[2];
                var token = new CancellationToken();
                   

                //await gor user to discnnect it
                var res = await webSocket.ReceiveAsync(Buffrer, token);
                //if (res.MessageType == WebSocketMessageType.Close)
                //    Console.WriteLine("disconneted");
                this.websockerConnectionService.removeConnectedUser(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }

          
        }

         

        
    }
}
