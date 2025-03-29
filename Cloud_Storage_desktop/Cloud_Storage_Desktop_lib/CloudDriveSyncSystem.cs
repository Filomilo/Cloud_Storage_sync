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
using Cloud_Storage_Common.Models;
using Lombok.NET;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Cloud_Storage_Desktop_lib
{
    internal class SyncTask
    {
        public SyncTask(Task task, CancellationTokenSource token, string file)
        {
            Task = task;
            Token = token;
            this.file = file;
        }

        public Task Task{
            get; set;
        }
        public string file { get; set; }
        public CancellationTokenSource Token { get; set; }
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

        private Configuration _Configuration = new Configuration();
        public Configuration Configuration
        {
            get { return _Configuration; }
        }

        private object _SyncCollectionLock = new object();
        private CloudDriveSyncSystem()
        {
            this._ServerConnection = new ServerConnection(this.Configuration.ApiUrl);
            this._Synctasks.CollectionChanged += RefreshQueue;
        }

        //Testt only do not use
        public CloudDriveSyncSystem(HttpClient client)
        {
            this._ServerConnection = new ServerConnection(client);

            _instance = this;
        }

        private ObservableCollection<SyncTask> _Synctasks =
            new ObservableCollection<SyncTask>(new List<SyncTask>());
        private Queue<SyncTask> _FileToSyncQueue = new Queue<SyncTask>();
        public void CancelSyncingAll()
        {
            throw new NotImplementedException("Not implmented");
        }

        private void _AddTaskToRun(SyncTask task)
        {
            logger.Log(LogLevel.Debug,"Added taks");
            this._Synctasks.Add(task);
            task.Task.Start();
        }

        private void _RemoveTaskToRun(SyncTask task)
        {

        }
        private void RefreshQueue(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action != NotifyCollectionChangedAction.Remove)
                return;
            lock (this._SyncCollectionLock)
            {
                if (this._Synctasks.Count < this.Configuration.MaxStimulationsFileSync && this._FileToSyncQueue.Count > 0)
                {

                    var FromQueue = this._FileToSyncQueue.Dequeue();
     

                }
                else
                {
                    logger.LogDebug("Cannot add syncing file");
                }
            }
           
        }
        private void _AddNewSyncTask(Task task, CancellationTokenSource token,string file)
        {
            lock (this._SyncCollectionLock)
            {
                if (_Synctasks.Count >= this.Configuration.MaxStimulationsFileSync)
                {
                    this._FileToSyncQueue.Enqueue(
                       new SyncTask(task, token, file));
                }
                else
                {
                    _AddTaskToRun(new SyncTask(task, token, file));
                }
            }
        }
        public void SyncFiles()
        {
            logger.Log(LogLevel.Information,$"Retrving files in location:{this.Configuration.StorageLocation} ");
            List<String> files = FileManager.GetAllFilePathInLocaation(this._Configuration.StorageLocation);
            foreach (String file in files)
            {
                CancellationTokenSource token = new CancellationTokenSource();
                Task syncFileTask = new Task(() =>
                {
                    try
                    {
                        this.SyncFile(file);
                    }
                    catch (Exception exception)
                    {
                        logger.Log(LogLevel.Error,$"Error while syncing file {exception.Message}");
                    }

                    lock (this._SyncCollectionLock)
                    {
                    SyncTask syncTask = this._Synctasks.First(x => x.file == file);
                    
                    if (syncTask!=null)
                    {
                        this._Synctasks.Remove(syncTask);
                    }
                    }
                   
                },token.Token);

                _AddNewSyncTask(syncFileTask, token,file);

            }

        }

        private void SyncFile(string filePath)
        {
            logger.Log(LogLevel.Information,$"Start sync: {filePath}");
            Thread.Sleep(100);
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
