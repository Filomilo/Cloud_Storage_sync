using System;
using System.Collections.Generic;
using System.IO;
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
            using (
                FileStream file = File.Open(
                    CloudDriveLogging.Instance.getLogFilePath(),
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.ReadWrite
                )
            )
            {
                file.SetLength(0);
            }
        }
    }
}
