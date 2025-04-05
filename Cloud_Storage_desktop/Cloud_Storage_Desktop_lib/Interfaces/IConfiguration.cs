namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public delegate void StorageLocationChangedDelgate(string newLocation);

    public interface IConfiguration
    {
        string ApiUrl { get; }
        int MaxStimulationsFileSync { get; }
        string DeviceUUID { get; }
        string StorageLocation { get; set; }
        string ToString();
    }
}
