using System.ComponentModel.DataAnnotations;
using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;
using Lombok.NET;
using Newtonsoft.Json;

namespace Cloud_Storage_Desktop_lib
{
    [AllArgsConstructor]
    [NoArgsConstructor]
    public partial class Configuration : IConfiguration
    {
        [JsonProperty("ApiUrl")]
        public string ApiUrl { get; set; } = "http://localhost:5087";

        [JsonProperty("MaxStimulationsFileSync")]
        public int MaxStimulationsFileSync { get; set; }

        [JsonProperty("StorageLocation")]
        [RegularExpression(
            @"^@""^[a-zA-Z]:\\(?:[a-zA-Z0-9 _-]+\\)*[a-zA-Z0-9 _-]+\.txt$""",
            ErrorMessage = "Path string doesn't match path syntax"
        )]
        public string StorageLocation { get; set; }

        public void LoadConfiguration()
        {
            string content;
            using (StreamReader sr = new StreamReader(GetConfigurationPath()))
            {
                content = sr.ReadToEnd();
            }
            IConfiguration config = JsonOperations.ObjectFromJSon<Configuration>(content);
            this.StorageLocation = config.StorageLocation;
            this.ApiUrl = config.ApiUrl;
            this.MaxStimulationsFileSync = config.MaxStimulationsFileSync;
        }

        public static IConfiguration InitConfig()
        {
            IConfiguration config = new Configuration();
            try
            {
                config.LoadConfiguration();
            }
            catch (Exception ex)
            {
                config.SaveConfiguration();
            }
            return config;
        }

        public void SaveConfiguration()
        {
            string path = GetConfigurationPath();
            if (!Directory.Exists(GetAppDirectory()))
            {
                Directory.CreateDirectory(GetAppDirectory());
            }
            using (StreamWriter sw = new StreamWriter(GetConfigurationPath()))
            {
                sw.Write(JsonOperations.jsonFromObject(this));
            }
        }

        private static string GetConfigurationPath()
        {
            return $"{GetAppDirectory()}\\config.json";
        }

        public static string GetAppDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                + "\\CloudDriveSync";
        }

        public override string ToString()
        {
            return $"ApiUrl: {ApiUrl}, MaxStimulationsFileSync: {MaxStimulationsFileSync}, StorageLocation: {StorageLocation}";
        }
    }
}
