using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    public enum CONFIGURAITON_TYPE
    {
        FILECONFIG,
        INMEM_CONFIG,
    }

    public static class ConfigurationFactory
    {
        public static CONFIGURAITON_TYPE type { get; set; } = CONFIGURAITON_TYPE.FILECONFIG;

        public static IConfiguration GetConfiguration()
        {
            switch (type)
            {
                case CONFIGURAITON_TYPE.FILECONFIG:
                    return Configuration.InitConfig();
                case CONFIGURAITON_TYPE.INMEM_CONFIG:
                    return new InMemoryConfiguration();
            }

            throw new ArgumentException("Not ahndled config type");
        }
    }
}
