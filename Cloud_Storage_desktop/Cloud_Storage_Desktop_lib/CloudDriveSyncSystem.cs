using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Lombok.NET;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib
{
    public partial class CloudDriveSyncSystem
    {
        private static ILogger logger = CloudDriveLogging.Instance.loggerFactory.CreateLogger(
            "CloudDriveSyncSystem"
        );
        private static CloudDriveSyncSystem _instance;
        public static CloudDriveSyncSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CloudDriveSyncSystem();
                }

                return _instance;
            }
        }
        private ServerConnection _ServerConnection;
        public ServerConnection ServerConnection
        {
            get { return _ServerConnection; }
        }

        private Configuration _Configuration = new Configuration();
        public Configuration Configuration
        {
            get { return _Configuration; }
        }

        private CloudDriveSyncSystem()
        {
            this._ServerConnection = new ServerConnection(this.Configuration.ApiUrl);
        }

        //Testt only do not use
        public CloudDriveSyncSystem(HttpClient client)
        {
            this._ServerConnection = new ServerConnection(client);

            _instance = this;
        }

        public void UploudFiles()
        {
            List<UploudFileData> filesToUploud = FileManager.GetAllFilesInLocationRelative(
                Configuration.StorageLocation
            );
            Console.WriteLine($"Found {filesToUploud.Count} files, attempting to uploud");
            for (var i = 0; i < filesToUploud.Count; i++)
            {
                Console.WriteLine(
                    $"Uplouding {filesToUploud[i].getFullFilePathForBasePath(this.Configuration.StorageLocation)}"
                );
                try
                {
                    this.ServerConnection.uploudFile(
                        filesToUploud[i],
                        FileManager.GetBytesOfFiles(
                            filesToUploud[i]
                                .getFullFilePathForBasePath(this.Configuration.StorageLocation)
                        )
                    );
                }
                catch (Exception ex)
                {
                    logger.Log(
                        LogLevel.Warning,
                        $"Failed to uploud file [{filesToUploud[i].getFullFilePathForBasePath(this.Configuration.StorageLocation)} :: CAUSE [{ex.Message}]]"
                    );
                }
            }
        }

        public void DownloadFiles()
        {
            List<FileData> filesOnCloud = this.GetListOfFilesOnCloud();
            foreach (FileData file in filesOnCloud)
            {
                FileManager.SaveFile(
                    file.getFullFilePathForBasePath(this.Configuration.StorageLocation),
                    this.ServerConnection.DownloadFlie(file.Id)
                );
            }
        }

        public List<FileData> GetListOfFilesOnCloud()
        {
            return this.ServerConnection.GetListOfFiles();
        }
    }
}
