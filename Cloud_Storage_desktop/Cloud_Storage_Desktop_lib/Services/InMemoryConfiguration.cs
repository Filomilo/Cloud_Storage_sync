using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class InMemoryConfiguration : IConfiguration
    {
        public string ApiUrl { get; set; } = "http://localhost:5087/";
        public int MaxStimulationsFileSync { get; set; } = 5;
        public string StorageLocation { get; set; }

        public void LoadConfiguration()
        {
            if (OnConfigurationChange != null)
            {
                OnConfigurationChange.Invoke();
            }
        }

        public void SaveConfiguration()
        {
            if (OnConfigurationChange != null)
            {
                OnConfigurationChange.Invoke();
            }
        }

        public event ConfigurationChange? OnConfigurationChange;
    }
}
