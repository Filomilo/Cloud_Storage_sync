using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Database;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_Server.Database;
using Microsoft.Win32;

namespace Cloud_Storage_Test
{
    public static class DummyManualTest
    {
        private static bool isRegistered = false;

        private static string email = "dummy1@wp.pl";
        private static string password = "ASda98sdASd70-87A9s65(**&";
        private static object isRegisteredLock = new object();

        public static void login(bool register)
        {
            if (register)
            {
                TestHelpers.ClearServerStorage();
                TestHelpers.ClearServerDb();
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
            IConfiguration config = ConfigurationFactory.GetConfiguration();
            config.ApiUrl = "http://localhost:5087/";
            config.MaxStimulationsFileSync = 6;
            config.SaveConfiguration();
        }

        public static void startDummy(int i, List<string> startingFiles)
        {
            DbContextGeneratorFactory.dbType = DBTYPE.INMEM;
            CredentialManageFactory.type = CREDENTIALMANGGETYPE.INMEMORY_CREDENTIALS;
            ConfigurationFactory.type = CONFIGURAITON_TYPE.INMEM_CONFIG;

            DateTime now = DateTime.Now;
            DateTime roundedToMinute = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                now.Hour,
                now.Minute,
                0
            );
            email = TestHelpers.getEmail(now.Minute);

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
