using Cloud_Storage_Server.Database.Models;
using System.Net.WebSockets;

namespace Cloud_Storage_Server.Services
{
    //public class UserConnection
    //{
    //    public Database.Models.User User { get; set; }
    //    public WebSocket WebSocket { get; set; }
    //}


    public interface IWebsockerConnectionService
    {
        void addConnetedUser(WebSocket webscoket,User user);
        void removeConnectedUser(WebSocket webSocket);
    }
    public class WebsockerConnectionService : IWebsockerConnectionService
    {
        private Dictionary<WebSocket, User> _users = new Dictionary<WebSocket, User>();
        public void addConnetedUser(WebSocket webscoket, User user)
        {
            _users.Add(webscoket, user);
        }

        public void removeConnectedUser(WebSocket webSocket)
        {
            _users.Remove(webSocket);
        }
    }
}
