using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;

namespace Cloud_Storage_Test
{
    public static class LogFileController
    {
        public static string GetLogFileContent()
        {
            string conetn = "";
            using (
                var stram = new StreamReader(
                    File.Open(
                        CloudDriveLogging.Instance.getLogFilePath(),
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite
                    )
                )
            )
            {
                conetn = stram.ReadToEnd();
            }

            return conetn;
        }

        public static void ClearlogFile()
        {
            if (!File.Exists(CloudDriveLogging.Instance.getLogFilePath()))
                return;
            File.Delete(CloudDriveLogging.Instance.getLogFilePath());
        }
    }
}
