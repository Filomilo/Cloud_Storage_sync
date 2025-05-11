using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using NUnit.Framework.Constraints;
using TimeoutException = System.TimeoutException;

namespace Cloud_Storage_desktop.Logic
{
    public class ServiceOperator
    {
        private string ServiceName = "CloudDriveService";

        private String ServicePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Service",
            "CloudDriveSyncService.exe"
        );

        private ServiceController getCloudDriveService()
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.FirstOrDefault(s => s.ServiceName == ServiceName);
        }

        private ServiceControllerStatus? ServiceStatus
        {
            get { return getCloudDriveService()?.Status; }
        }

        public bool Exist
        {
            get { return getCloudDriveService() != null; }
        }

        public bool IsServiceRunning()
        {
            return ServiceStatus == ServiceControllerStatus.Running;
        }

        public async Task StartService()
        {
            try
            {
                Awaiters.AwaitNotThrows(
                    () =>
                    {
                        runServiceOperation("start", false);
                    },
                    5000
                );

                Awaiters.AwaitTrue(() => IsServiceRunning(), 5000);
            }
            catch (TimeoutException ex)
            {
                this.StopService();
                throw new Exception("Could not start service specifed amount of time");
            }
        }

        private async Task runServiceOperation(string operations, bool includebinpath = true)
        {
            string program = "sc.exe";
            string args =
                $"{operations} {this.ServiceName} "
                + (includebinpath ? $"binPath= \"{ServicePath}\"" : "");
            Process process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = args,
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            );

            process.WaitForExit(5000);
            if (process.ExitCode == 5)
            {
                throw new Exception(" Permission denied. Please run as administrator.");
            }
            if (process.ExitCode != 0)
            {
                int exitCode = process.ExitCode;
                string output = process.StandardOutput.ReadToEnd();
                string errorout = process.StandardError.ReadToEnd();
                process.Close();

                throw new Exception(
                    $" {program} {args} -- {exitCode}: \n {output} \n\n {errorout} "
                );
            }
        }

        public async Task CreateService()
        {
            runServiceOperation("create");
            try
            {
                Awaiters.AwaitTrue(() => Exist);
            }
            catch (TimeoutException ex)
            {
                this.DeleteService();
                throw new Exception("Couldn't start servce within sepcifed aomunt of time");
            }
        }

        public async Task UpdateService()
        {
            await this.StopService();
            await this.DeleteService();
            await this.CreateService();
            await this.StartService();
        }

        public async Task StopService()
        {
            runServiceOperation("stop", false);
            try
            {
                Awaiters.AwaitTrue(() => !IsServiceRunning());
            }
            catch (TimeoutException ex)
            {
                throw new Exception("Couldnt 'stop serivce within specifed amount of time");
            }
        }

        public async Task DeleteService()
        {
            try
            {
                if (IsServiceRunning())
                    StopService();
                runServiceOperation("delete");
                Awaiters.AwaitTrue(() => !Exist, 5000);
            }
            catch (TimeoutException ex)
            {
                throw new Exception("Couldn't delete service within sepcifed amount of time");
                ;
            }
        }
    }
}
