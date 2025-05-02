namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public delegate void ConfigurationChange();

    public interface IConfiguration
    {
        string ApiUrl { get; set; }
        int MaxStimulationsFileSync { get; }
        string StorageLocation { get; set; }
        string ToString();

        void LoadConfiguration();
        void SaveConfiguration();
        event ConfigurationChange OnConfigurationChange;
    }
}
