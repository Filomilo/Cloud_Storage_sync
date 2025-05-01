using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using NUnit.Framework.Constraints;

namespace Cloud_Storage_desktop.Logic
{
    public class ServiceOperator
    {
        private string ServiceName = "CloudDriveService";

        private String ServicePath =
            AppDomain.CurrentDomain.BaseDirectory + "Service\\CloudDriveSyncService.exe";

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

        public void StartService()
        {
            runServiceOperation("start", false);
            Awaiters.AwaitTrue(() => IsServiceRunning());
        }

        private void runServiceOperation(string operations, bool includebinpath = true)
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

        public void CreateService()
        {
            runServiceOperation("create");
            Awaiters.AwaitTrue(() => Exist);
        }

        public void UpdateService()
        {
            this.StopService();
            this.DeleteService();
            this.CreateService();
            this.StartService();
        }

        public void StopService()
        {
            runServiceOperation("stop", false);
            Awaiters.AwaitTrue(() => !IsServiceRunning());
        }

        public void DeleteService()
        {
            if (IsServiceRunning())
                StopService();
            runServiceOperation("delete");
            Awaiters.AwaitTrue(() => !Exist);
        }
    }
}
