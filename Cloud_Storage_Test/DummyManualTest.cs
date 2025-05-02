using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Server.Database;
using Microsoft.Win32;

namespace Cloud_Storage_Test
{
    public static class DummyManualTest
    {
        private static bool isRegistered = false;

        private static string email = "mailwp@wp.pl";
        private static string password = "password+12313ABC";
        private static object isRegisteredLock = new object();

        public static void login(bool register)
        {
            if (register)
            {
                isRegistered = true;
                CloudDriveSyncSystem.Instance.ServerConnection.Register(email, password);
                return;
            }

            TestHelpers.EnsureNotThrows(() =>
            {
                CloudDriveSyncSystem.Instance.ServerConnection.login(email, password);
            });
        }

        static void setupDb()
        {
            using (var ctx = new SqliteDataBaseContextGenerator().GetDbContext())
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }
        }

        static void setupConifg()
        {
            IConfiguration config = Configuration.InitConfig();
            config.ApiUrl = "http://localhost:5087/";
            config.SaveConfiguration();
        }

        public static void startDummy(int i, List<string> startingFiles)
        {
            setupConifg();
            if (i == 0)
            {
                setupDb();
            }
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
            CloudDriveSyncSystem.Instance.Configuration.StorageLocation = (path);
            CloudDriveSyncSystem.Instance.Configuration.SaveConfiguration();
            ;
            login(i == 0);
            while (true) { }
        }
    }
}
