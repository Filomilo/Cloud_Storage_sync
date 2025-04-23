using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib;
using Microsoft.Win32;

namespace Cloud_Storage_Test
{
    public static class DummyManualTest
    {
        private static bool isRegistered = false;

        private static string email = "mailwp@wp.pl";
        private static string password = "password+12313ABC";
        private static object isRegisteredLock = new object();

        public static void register(bool register)
        {
            //if (register)
            //{
            //    isRegistered = true;
            //    CloudDriveSyncSystem.Instance.ServerConnection.Register(email, password);
            //    return;
            //}

            TestHelpers.EnsureNotThrows(() =>
            {
                CloudDriveSyncSystem.Instance.ServerConnection.login(email, password);
            });
        }

        public static void startDummy(int i, List<string> startingFiles)
        {
            TestHelpers.EnsureNotThrows(() =>
            {
                CloudDriveSyncSystem.Instance.ServerConnection.CheckIfHelathy();
            });
            string path = TestHelpers.getDummyLocation(i);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            foreach (string file in startingFiles)
            {
                string filePath = Path.Combine(path, file);
                using (var fileStream = File.OpenWrite(filePath))
                {
                    fileStream.Write(new byte[100], 0, 100);
                }
            }
            HttpClient client = new HttpClient();
            CloudDriveSyncSystem.Instance.SetStorageLocation(path);
            register(i == 0);
            while (true) { }
        }
    }
}
