using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Security.Policy;
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
        public string ApiUrl { get; set; }

        [JsonProperty("MaxStimulationsFileSync")]
        public int MaxStimulationsFileSync { get; set; }

        [JsonProperty("StorageLocation")]
        //[RegularExpression(
        //    @"^@""^[a-zA-Z]:\\(?:[a-zA-Z0-9 _-]+\\)*[a-zA-Z0-9 _-]+\.txt$""",
        //    ErrorMessage = "Path string doesn't match path syntax"
        //)]
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
            if (!Directory.Exists(SharedData.GetAppDirectory()))
            {
                Directory.CreateDirectory(SharedData.GetAppDirectory());
            }
            using (StreamWriter sw = new StreamWriter(GetConfigurationPath()))
            {
                sw.Write(JsonOperations.jsonFromObject(this));
            }
        }

        private static string GetConfigurationPath()
        {
            return $"{SharedData.GetAppDirectory()}\\config.json";
        }

        public override string ToString()
        {
            return $"ApiUrl: {ApiUrl}, MaxStimulationsFileSync: {MaxStimulationsFileSync}, StorageLocation: {StorageLocation}";
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (
                    uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps
                );
        }

        public void ValidateConfiguration()
        {
            if (!Directory.Exists(StorageLocation))
            {
                throw new ValidationException("Storage location is not a directory");
            }

            if (IsValidUrl(ApiUrl))
            {
                throw new ValidationException("ApiUrl is not a valid URL");
            }
            if (MaxStimulationsFileSync < 1)
            {
                throw new ValidationException("MaxStimulationsFileSync must be greater than 0");
            }
        }
    }
}
