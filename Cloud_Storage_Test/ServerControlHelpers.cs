using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_desktop.Logic;
using Lombok.NET;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework.Internal;

namespace Cloud_Storage_Test
{
    [Singleton]
    public partial class ServerControlHelpers
    {
        private Process process;

        public void StartServer()
        {
            string serverExePath =
                "..\\..\\..\\..\\Cloud_Storage_Server\\bin\\Release\\net8.0\\Cloud_Storage_Server.exe";

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
            TestHelpers.EnsureTrue(() =>
            {
                return serverConnection.CheckIfHelathy();
            });
        }

        public void StopServer()
        {
            if (process != null)
            {
                process.Kill();
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
