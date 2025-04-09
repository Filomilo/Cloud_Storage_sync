using System.Net.WebSockets;

namespace Cloud_Storage_Server.Interfaces
{
    public interface IConnectedDevice
    {
        object DeviceId { get; }
        object UserId { get; }
        WebSocket WebSocket { get; }
    }

    public interface IWebsocketConnectedController
    {
        void AddDevice(IConnectedDevice device);
        void RemoveDevice(object deviceId);
        IEnumerable<IConnectedDevice> GetAllConnectedDevices();
        void SendMessageToAllConnectedDevices(object message);
        void SendMessageToDevice(object DeviceId, object message);
        void SendMessageToUser(object userID, object message);
        void DisconnectDevices();
    }
}
