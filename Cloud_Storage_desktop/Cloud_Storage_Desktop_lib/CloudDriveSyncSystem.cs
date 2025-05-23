﻿using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Database;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Microsoft.Extensions.Logging;
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
        private IServerConnection _ServerConnection;
        public IServerConnection ServerConnection
        {
            get { return _ServerConnection; }
        }

        public IConfiguration Configuration
        {
            get { return _Configuration; }
        }
        private IConfiguration _Configuration = ConfigurationFactory.GetConfiguration();

        public IFileRepositoryService _FileRepositoryService;

        public ICredentialManager CredentialManager
        {
            get { return _CredentialManager; }
        }
        private ICredentialManager _CredentialManager =
            CredentialManageFactory.GetCredentialManager();

        public IFileSyncService FileSyncService;
        public IFIleSystemWatcher SystemWatcher;
        public IServerFilesStateWatcher _filesStateWatcher;
        private WebSocketWrapper _WebSocketWrapper = new WebSocketWrapper();
        #endregion

        private void SetupSererConeciotn()
        {
            logger.LogTrace($"SetupSererConeciotn");
            if (this._ServerConnection == null)
            {
                this._ServerConnection = new ServerConnection(
                    this._Configuration.ApiUrl,
                    this.CredentialManager,
                    new WebSocketWrapper()
                );
            }
            else
            {
                this._ServerConnection.AdressChange(this._Configuration.ApiUrl);
            }
        }

        private CloudDriveSyncSystem()
        {
            SetupSererConeciotn();
            this._FileRepositoryService = new FileRepositoryService(
                DbContextGeneratorFactory.GetContextGenerator()
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
            this.SystemWatcher.OnRenamedEventHandler += this.FileSyncService.OnLocallyOnRenamed;
            this.Configuration.OnConfigurationChange += ReloadSyncSystem;
            this._filesStateWatcher = new ServerFileStateWatcher(this.ServerConnection);
            this.ServerConnection.ServerWerbsocketHadnler += message =>
            {
                this._filesStateWatcher.RefreshList();
            };
        }

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
            SetupSystemWachter();
            _instance = this;
        }
        //Testt only do not use

        #endregion

        private void SetupSystemWachter()
        {
            logger.LogInformation("SetupSystemWachter");
            this.SystemWatcher.Directory = this._Configuration.StorageLocation;
        }

        private void ReloadSyncSystem()
        {
            logger.LogInformation("Reloading sync system");
            this.FileSyncService.StopAllSync();
            this.FileSyncService.ResetSync();

            SetupSystemWachter();
            SetupSererConeciotn();
            this.FileSyncService.StartSync();
        }

        public void Dispose()
        {
            this._ServerConnection.Dispose();
            this.SystemWatcher.Dispose();
            this.FileSyncService.Dispose();
        }
    }
}
