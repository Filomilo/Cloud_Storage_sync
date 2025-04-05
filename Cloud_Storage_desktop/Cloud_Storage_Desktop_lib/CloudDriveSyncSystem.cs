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
using FileSystemWatcher = Cloud_Storage_Desktop_lib.Services.FileSystemWatcher;

namespace Cloud_Storage_Desktop_lib
{
    internal class SyncTask : ITaskToRun
    {
        private String file;
        private Action action;
        public object Id
        {
            get { return file; }
        }

        public Action ActionToRun
        {
            get { return action; }
        }

        public SyncTask(String file, Action action)
        {
            this.file = file;
            this.action = action;
        }
    }

    public partial class CloudDriveSyncSystem
    {
        private static ILogger logger = CloudDriveLogging.Instance.GetLogger(
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

        public IConfiguration Configuration
        {
            get { return _Configuration; }
        }
        private IConfiguration _Configuration = new Configuration();

        public ICredentialManager CredentialManager
        {
            get { return _CredentialManager; }
        }
        private ICredentialManager _CredentialManager = new CredentialManager();

        //public IConfiguration Configuration
        //{
        //    get { return _Configuration; }
        //}

        //private ITaskRunController _TaskRunnerController;
        public IFileSyncService FileSyncService;
        public IFIleSystemWatcher SystemWatcher;

        private CloudDriveSyncSystem()
        {
            this._ServerConnection = new ServerConnection(
                this._Configuration.ApiUrl,
                this.CredentialManager
            );
            _init();
        }

        private void _init()
        {
            this.SystemWatcher = new FileSystemWatcher();

            this.FileSyncService = new SyncFileService(this.Configuration, this._ServerConnection);
            this.SystemWatcher.OnDeletedEventHandler += this.FileSyncService.OnLocallyDeleted;
            this.SystemWatcher.OnChangedEventHandler += this.FileSyncService.OnLocallyChanged;
            this.SystemWatcher.OnCreatedEventHandler += this.FileSyncService.OnLocallyCreated;
            this.SystemWatcher.OnRenamedEventHandler += this.FileSyncService.OnLocallyOnRenamed;
        }

        //private IHandler _FileSyncHandler;

        #region Test only constuctors
        public CloudDriveSyncSystem(HttpClient client)
        {
            this._ServerConnection = new ServerConnection(client, this.CredentialManager);

            _instance = this;
        }

        public CloudDriveSyncSystem(
            HttpClient client,
            IConfiguration configuration,
            ICredentialManager credentialManager
        )
        {
            this._ServerConnection = new ServerConnection(client, credentialManager);
            this._Configuration = configuration;
            _init();
            _setupStorgeDir();

            _instance = this;
        }
        //Testt only do not use

        #endregion

        private void _setupStorgeDir()
        {
            this.SystemWatcher.Directory = this._Configuration.StorageLocation;
            this.FileSyncService.StopAllSync();
            this.FileSyncService.StartSync();
        }

        public void SetStorageLocation(string dir)
        {
            this._Configuration.StorageLocation = dir;
            _setupStorgeDir();
        }
    }
}
