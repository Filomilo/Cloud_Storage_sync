using Cloud_Storage_Server.Interfaces;

namespace Cloud_Storage_Server.Services
{
    public class ServerConfig : IServerConfig
    {
        public string StorageLocation { get; set; } = "dataStorage\\";
        public ulong BackupMaxSize { get; set; } = 1024 * 1024 * 100; //100Mb
    }
}
