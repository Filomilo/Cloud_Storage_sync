using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Common
{
    public static class SharedData
    {
        public static string GetAppDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CloudDriveSync"
            );
        }
    }
}
