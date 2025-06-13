using System.Diagnostics;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_desktop.Logic;
using Cloud_Storage_Server.Database;
using Lombok.NET;

namespace Cloud_Storage_Test
{
    [Singleton]
    public partial class ServerControlHelpers
    {
        private Process process;

        private void ensureAccesToDb()
        {
            DatabaseContextSqLite db = new DatabaseContextSqLite();
            db.Database.EnsureDeleted();
        }

        public void StartServer()
        {
            ensureAccesToDb();
            string serverExePath =
                "..\\..\\..\\..\\Cloud_Storage_Server\\bin\\Release\\net8.0\\win-x64\\Cloud_Storage_Server.exe";

            process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = serverExePath,
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                }
            );

            ServerConnection serverConnection = new ServerConnection(
                GetIpConnection(),
                new TestCredentialMangager(),
                new NullWebSocket()
            );
            TestHelpers.EnsureTrue(
                () =>
                {
                    return serverConnection.CheckIfHelathy();
                },
                10000
            );
        }

        public void StopServer()
        {
            if (process != null)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        public string GetIpConnection()
        {
            return "http://localhost:5000";
        }

        ~ServerControlHelpers()
        {
            StopServer();
        }
    }
}
