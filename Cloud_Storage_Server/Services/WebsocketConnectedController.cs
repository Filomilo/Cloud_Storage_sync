using System.Net.WebSockets;
using System.Text;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Interfaces;

namespace Cloud_Storage_Server.Services
{
    class DeviceSocket : IConnectedDevice
    {
        private Device _device;
        private WebSocket _socket;

        public DeviceSocket(Device device, WebSocket webSocket)
        {
            _device = device;
            _socket = webSocket;
        }

        public object DeviceId
        {
            get { return _device?.Id; }
        }

        public object UserId
        {
            get { return _device.OwnerId; }
        }

        public WebSocket WebSocket
        {
            get { return _socket; }
        }
    }

    public class WebsocketConnectedController : IWebsocketConnectedController
    {
        private object Locker = new object();

        Dictionary<object, IConnectedDevice> _connectedDevices =
            new Dictionary<object, IConnectedDevice>();

        public void AddDevice(IConnectedDevice device)
        {
            ValidateConnections();
            if (_connectedDevices.ContainsKey(device.DeviceId))
            {
                RemoveDevice(device.DeviceId);
            }
            lock (Locker)
            {
                _connectedDevices.Add(device.DeviceId, device);
            }
        }

        public void RemoveDevice(object deviceId)
        {
            lock (Locker)
            {
                _connectedDevices.Remove(deviceId);
            }
        }

        public void DisconnectDevices()
        {
            lock (Locker)
            {
                _connectedDevices.Clear();
            }
        }

        public IEnumerable<IConnectedDevice> GetAllConnectedDevices()
        {
            return _connectedDevices.Values;
        }

        public void SendMessageToAllConnectedDevices(object message, string excludingDevice = null)
        {
            ValidateConnections();
            lock (Locker)
            {
                _connectedDevices
                    .Values.ToList()
                    .ForEach(device =>
                    {
                        if (device.DeviceId.ToString() == excludingDevice)
                            return;
                        device.WebSocket.SendAsync(
                            new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToString())),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None
                        );
                    });
            }
        }

        private void ValidateConnections()
        {
            lock (Locker)
            {
                this._connectedDevices.Values.Where(x =>
                        x.WebSocket.State == WebSocketState.Closed
                        || x.WebSocket.State == WebSocketState.Aborted
                    )
                    .ToList()
                    .ForEach(device =>
                    {
                        RemoveDevice(device.DeviceId);
                    });
            }
        }

        public void SendMessageToDevice(object DeviceId, object message)
        {
            ValidateConnections();
            lock (Locker)
            {
                _connectedDevices.TryGetValue(DeviceId, out var device);
                if (device != null)
                {
                    device.WebSocket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToString())),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
            }
        }

        public void SendMessageToUser(object userID, object message)
        {
            ValidateConnections();
            lock (Locker)
            {
                _connectedDevices
                    .Values.ToList()
                    .ForEach(device =>
                    {
                        if (device.UserId.Equals(userID))
                        {
                            device.WebSocket.SendAsync(
                                new ArraySegment<byte>(
                                    Encoding.UTF8.GetBytes(JsonOperations.jsonFromObject(message))
                                ),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None
                            );
                        }
                    });
            }
        }
    }
}
