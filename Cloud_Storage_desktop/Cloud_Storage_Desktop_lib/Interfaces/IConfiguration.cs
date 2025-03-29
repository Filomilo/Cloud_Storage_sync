namespace Cloud_Storage_Desktop_lib.Interfaces;

public interface IConfiguration
{
    string ApiUrl { get; }
    int MaxStimulationsFileSync { get; }
    string StorageLocation { get; set; }
    string ToString();
}