using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_Desktop_lib.SyncingHandlers;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Cloud_Storage_Desktop_lib
{
    internal class SyncTask : ITaskToRun
    {
        private String file;
        private Action action;
        public object Id
        {
            get
            {
                return file;
            }
        }

        public Action ActionToRun
        {
            get
            {
                return action;
            }
        }

        public SyncTask(String file, Action action)
        {
            this.file=file;
            this.action = action;
        }
    }

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

        private IConfiguration _Configuration = new Configuration();
        public IConfiguration Configuration
        {
            get { return _Configuration; }
        }

        private ITaskRunController _TaskRunnerController;
        private CloudDriveSyncSystem()
        {
            this._ServerConnection = new ServerConnection(this.Configuration.ApiUrl);
            this._TaskRunnerController=new RunningTaskController(this.Configuration);
            _FileSyncHandler = new PrepareFileSyncData(_Configuration);
        }


        private IHandler _FileSyncHandler;


        //Testt only do not use
        public CloudDriveSyncSystem(HttpClient client)
        {
            this._ServerConnection = new ServerConnection(client);

            _instance = this;
        }


    
        public void SyncFiles()
        {
            logger.Log(LogLevel.Information,$"Retrving files in location:{this.Configuration.StorageLocation} ");
            List<String> files = FileManager.GetAllFilePathInLocaation(this._Configuration.StorageLocation);
            foreach (String file in files)
            {
                CancellationTokenSource token = new CancellationTokenSource();
                this._TaskRunnerController.AddTask(new SyncTask(
                    file, () =>
                    {
                        try
                        {
                            this.SyncFile(file);
                        }
                        catch (Exception exception)
                        {
                            logger.Log(LogLevel.Error, $"Error while syncing file {exception.Message}");
                        }
                    })
                    );

            }

        }

        private void SyncFile(string filePath)
        {
            logger.Log(LogLevel.Information,$"Start sync: {filePath}");
            Thread.Sleep(5000);
            object res = this._FileSyncHandler.Handle(filePath);
            logger.LogDebug("FileSyync Handler result: "+ res.ToString());
            logger.Log(LogLevel.Information, $"Finished sync: {filePath}");
        }

        //public void UploudFiles()
        //{
        //    List<UploudFileData> filesToUploud = FileManager.GetAllFilesInLocationRelative(
        //        Configuration.StorageLocation
        //    );
        //    Console.WriteLine($"Found {filesToUploud.Count} files, attempting to uploud");
        //    for (var i = 0; i < filesToUploud.Count; i++)
        //    {
        //        Console.WriteLine(
        //            $"Uplouding {filesToUploud[i].getFullFilePathForBasePath(this.Configuration.StorageLocation)}"
        //        );
        //        try
        //        {
        //            this.ServerConnection.uploudFile(
        //                filesToUploud[i],
        //                FileManager.GetBytesOfFiles(
        //                    filesToUploud[i]
        //                        .getFullFilePathForBasePath(this.Configuration.StorageLocation)
        //                )
        //            );
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Log(
        //                LogLevel.Warning,
        //                $"Failed to uploud file [{filesToUploud[i].getFullFilePathForBasePath(this.Configuration.StorageLocation)} :: CAUSE [{ex.Message}]]"
        //            );
        //        }
        //    }
        //}

        //public void DownloadFiles()
        //{
        //    List<FileData> filesOnCloud = this.GetListOfFilesOnCloud();
        //    foreach (FileData file in filesOnCloud)
        //    {
        //        FileManager.SaveFile(
        //            file.getFullFilePathForBasePath(this.Configuration.StorageLocation),
        //            this.ServerConnection.DownloadFlie(file.Id)
        //        );
        //    }
        //}

        public List<FileData> GetListOfFilesOnCloud()
        {
            return this.ServerConnection.GetListOfFiles();
        }
    }
}
