﻿using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    public class LocalAndServerFileData
    {
        public List<FileData> LocalFiles;
        public List<SyncFileData> CloudFiles;

        public LocalAndServerFileData(List<FileData> LocalFiles, List<SyncFileData> CloudFiles)
        {
            this.CloudFiles = CloudFiles;
            this.LocalFiles = LocalFiles;
        }
    }

    public class GetLocalAndServerFileListHadndler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IServerConnection _connection;
        private IFileSyncService _fileSyncService;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("PerFileInitialSyncHandler");

        public GetLocalAndServerFileListHadndler(
            IConfiguration configuration,
            IServerConnection serverConnection,
            IFileSyncService fileSyncService
        )
        {
            this._configuration = configuration;
            this._connection = serverConnection;
            this._fileSyncService = fileSyncService;
        }

        public override object Handle(object request)
        {
            List<FileData> LocalFileData = FileManager.GetAllFilesInLocationRelative(
                this._configuration.StorageLocation
            );
            List<SyncFileData> CloudFilesData = new List<SyncFileData>();
            if (this._fileSyncService.Active)
            {
                CloudFilesData = _connection.GetAllCloudFilesInfo();
            }
            LocalAndServerFileData LocalAndServerFileData = new LocalAndServerFileData(
                LocalFileData,
                CloudFilesData
            );

            if (this._nextHandler != null)
                return this._nextHandler.Handle(LocalAndServerFileData);
            return LocalAndServerFileData;
        }
    }
}
