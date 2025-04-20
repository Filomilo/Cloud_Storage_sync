namespace Cloud_Storage_Server.Interfaces
{
    public interface IServerConfig
    {
        string StorageLocation { get; set; }
        ulong BackupMaxSize { get; set; }
    }
}
