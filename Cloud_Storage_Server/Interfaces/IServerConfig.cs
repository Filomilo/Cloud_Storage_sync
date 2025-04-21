namespace Cloud_Storage_Server.Interfaces
{
    public interface IServerConfig
    {
        string StorageLocation { get; set; }
        long BackupMaxSize { get; set; }
    }
}
