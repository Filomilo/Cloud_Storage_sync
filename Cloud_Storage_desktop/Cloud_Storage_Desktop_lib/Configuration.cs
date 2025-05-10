using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Security.Policy;
using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Mocks;
using Cloud_Storage_desktop.Logic;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cloud_Storage_Desktop_lib
{
    [AllArgsConstructor]
    [NoArgsConstructor]
    public partial class Configuration : IConfiguration
    {
        private ILogger _logger = CloudDriveLogging.Instance.GetLogger("Configuration");

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
            _logger.LogDebug("Load configuraiotn");
            string content;
            using (StreamReader sr = new StreamReader(GetConfigurationPath()))
            {
                content = sr.ReadToEnd();
            }
            IConfiguration config = JsonOperations.ObjectFromJSon<Configuration>(content);
            this.StorageLocation = config.StorageLocation;
            this.ApiUrl = config.ApiUrl;
            this.MaxStimulationsFileSync = config.MaxStimulationsFileSync;
            ActivateOnConfigChange();
        }

        public static IConfiguration InitConfig()
        {
            IConfiguration config = new Configuration();
            try
            {
                config.LoadConfiguration();
                if (config.ApiUrl == null)
                {
                    config.ApiUrl = "";
                    config.SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                config.SaveConfiguration();
            }

            return config;
        }

        public void SaveConfiguration()
        {
            try
            {
                string path = GetConfigurationPath();
                if (!Directory.Exists(SharedData.GetAppDirectory()))
                {
                    Directory.CreateDirectory(SharedData.GetAppDirectory());
                }
                Awaiters.AwaitNotThrows(() =>
                {
                    using (StreamWriter sw = new StreamWriter(GetConfigurationPath()))
                    {
                        sw.Write(JsonOperations.jsonFromObject(this));
                    }
                });

                ActivateOnConfigChange();
            }
            catch (TimeoutException ex)
            {
                throw new Exception("Couldn't acces file to save configuration");
            }
        }

        private void ActivateOnConfigChange()
        {
            if (OnConfigurationChange != null)
            {
                OnConfigurationChange.Invoke();
            }
        }

        public event ConfigurationChange? OnConfigurationChange;

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
            try
            {
                ServerConnection serverConnection = new ServerConnection(
                    this.ApiUrl,
                    new NullCredentialMange(),
                    new NullWebSocket()
                );
                return serverConnection.CheckIfHelathy();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void ValidateConfiguration()
        {
            if (!Directory.Exists(StorageLocation))
            {
                throw new ValidationException("Storage location is not a directory");
            }

            if (!IsValidUrl(ApiUrl))
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
