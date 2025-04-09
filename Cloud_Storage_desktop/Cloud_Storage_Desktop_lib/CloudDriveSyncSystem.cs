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
    public partial class CloudDriveSyncSystem
    {
        #region INterfaces

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

        private IFileRepositoryService _FileRepositoryService = new FileRepositoryService();

        public ICredentialManager CredentialManager
        {
            get { return _CredentialManager; }
        }
        private ICredentialManager _CredentialManager = new CredentialManager();

        public IFileSyncService FileSyncService;
        public IFIleSystemWatcher SystemWatcher;
        #endregion
        private CloudDriveSyncSystem()
        {
            this._ServerConnection = new ServerConnection(
                this._Configuration.ApiUrl,
                this.CredentialManager,
                new WebSocketWrapper()
            );
            _init();
        }

        private void _init()
        {
            this.SystemWatcher = new FileSystemWatcher();

            this.FileSyncService = new SyncFileService(
                this.Configuration,
                this._ServerConnection,
                this._FileRepositoryService
            );
            this.SystemWatcher.OnDeletedEventHandler += this.FileSyncService.OnLocallyDeleted;
            this.SystemWatcher.OnChangedEventHandler += this.FileSyncService.OnLocallyChanged;
            this.SystemWatcher.OnCreatedEventHandler += this.FileSyncService.OnLocallyCreated;
            this.SystemWatcher.OnRenamedEventHandler += this.FileSyncService.OnLocallyOnRenamed;
        }

        //private IHandler _FileSyncHandler;

        #region Test only constuctors
        public CloudDriveSyncSystem(HttpClient client)
        {
            this._ServerConnection = new ServerConnection(
                client,
                this.CredentialManager,
                new WebSocketWrapper()
            );

            _instance = this;
        }

        public CloudDriveSyncSystem(
            HttpClient client,
            IWebSocketWrapper WebSocketWrapper,
            IConfiguration configuration,
            ICredentialManager credentialManager,
            IFileRepositoryService localFileRepositoryService
        )
        {
            this._CredentialManager = credentialManager;
            this._ServerConnection = new ServerConnection(
                client,
                credentialManager,
                WebSocketWrapper
            );
            this._Configuration = configuration;
            this._FileRepositoryService = localFileRepositoryService;
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
