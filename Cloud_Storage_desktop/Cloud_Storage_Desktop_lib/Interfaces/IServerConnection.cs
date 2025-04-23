using System.Net.WebSockets;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public delegate void OnConnectionStateChange(bool isConnected);

    public delegate void OnServerWebSockerMessage(WebSocketMessage message);

    public interface IServerConnection
    {
        bool CheckIfAuthirized();
        bool CheckIfHelathy();
        Stream DownloadFile(Guid id);
        List<SyncFileData> GetAllCloudFilesInfo();
        List<SyncFileData> GetListOfFiles();
        void login(string email, string pass);
        void Logout();
        void Register(string email, string pass);
        void UploudFile(UploudFileData fileData, Stream value);
        void UpdateFileData(UpdateFileDataRequest file);
        event OnConnectionStateChange ConnectionChangeHandler;
        event OnServerWebSockerMessage ServerWerbsocketHadnler;
        WebSocketState WebSocketState { get; }
        void DeleteFile(string relativePath);
        void SetFileVersion(Guid id, ulong version);
    }
}
