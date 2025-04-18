using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.SyncingHandlers;
using Cloud_Storage_Server.Handlers;

namespace Cloud_Storage_Desktop_lib.Services
{
    class ClientChainOfResponsibilityRepository : IClientChainOfResponsibilityRepository
    {
        private ITaskRunController _taskRunController;
        private IServerConnection _serverConnection;
        private IFileRepositoryService _fileRepositoryService;
        private IConfiguration _configuration;
        private IFileSyncService _fileSyncService;

        public ClientChainOfResponsibilityRepository(
            ITaskRunController taskRunController,
            IServerConnection serverConnection,
            IFileRepositoryService fileRepositoryService,
            IConfiguration configuration,
            IFileSyncService fileSyncService
        )
        {
            _taskRunController = taskRunController;
            _serverConnection = serverConnection;
            _fileRepositoryService = fileRepositoryService;
            _configuration = configuration;
            _fileSyncService = fileSyncService;

            OnLocallyFileChangeHandler = CreateOnLocallyFileChangeHandler();
            OnLocalyFileRenamedHandler = CreateOnLocalyFileRenamedHandler();
            OnLocalyFileDeletedHandler = CreateOnLocalyFileDeletedHandler();
            OnCloudFileChangeHandler = CreateOnnCloudFileChangeHandler();
            OnCloudFileRenamedHandler = CreateOnCloudFileRenamedHandler();
            OnCloudFileCreatedHandler = CreateOnCloudFileCreatedHandler();
            OnCloudFileDeletedHandler = CreateOnCloudFileDeletedHandler();
            InitlalSyncHandler = CreateInitialSyncHandler();
        }

        private IHandler? CreateInitialSyncHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(
                    new GetLocalAndServerFileListHadndler(
                        _configuration,
                        _serverConnection,
                        _fileSyncService
                    )
                )
                .Next(new DeleteCloudLocalFilesHandler())
                .Next(
                    new DownloadMissingFilesHandler(
                        _configuration,
                        _serverConnection,
                        _taskRunController,
                        _fileRepositoryService
                    )
                )
                .Next(
                    new PerFileInitialSyncHandler(
                        _configuration,
                        _serverConnection,
                        _taskRunController,
                        _fileRepositoryService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnCloudFileDeletedHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new ValidateIfFileAlreadyExisitInDataBase(_fileRepositoryService))
                .Next(
                    new RenameFileOnUpdateHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(
                    new DownloadNewFIleHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(
                    new DeleteUpdateFileHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnCloudFileCreatedHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new ValidateIfFileAlreadyExisitInDataBase(_fileRepositoryService))
                .Next(
                    new RenameFileOnUpdateHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(
                    new DownloadNewFIleHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(
                    new DeleteUpdateFileHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnCloudFileRenamedHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new ValidateIfFileAlreadyExisitInDataBase(_fileRepositoryService))
                .Next(
                    new RenameFileOnUpdateHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(
                    new DownloadNewFIleHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(
                    new DeleteUpdateFileHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnnCloudFileChangeHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new ValidateIfFileAlreadyExisitInDataBase(_fileRepositoryService))
                .Next(
                    new RenameFileOnUpdateHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(
                    new DownloadNewFIleHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(
                    new DeleteUpdateFileHandler(
                        this._taskRunController,
                        this._serverConnection,
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnLocalyFileDeletedHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(
                    new RemoveFileFromDatabaseHandler(
                        this._configuration,
                        this._fileRepositoryService
                    )
                )
                .Next(new LocallyDeletedFileHandler(this._configuration, this._serverConnection))
                .Build();
        }

        private IHandler? CreateOnLocalyFileRenamedHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(
                    new UpdateDataBaseFileNameHandler(
                        this._fileRepositoryService,
                        this._configuration
                    )
                )
                .Next(new SendLocalFileUpdateToServer(this._serverConnection))
                .Build();
        }

        private IHandler? CreateOnLocallyFileChangeHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new PrepareFileSyncData(this._configuration))
                .Next(
                    new AddFileToDataBaseHandler(this._configuration, this._fileRepositoryService)
                )
                .Next(
                    new UploadNewFileHandler(
                        this._configuration,
                        this._serverConnection,
                        this._taskRunController,
                        this._fileRepositoryService
                    )
                )
                .Build();
        }

        public IHandler InitlalSyncHandler { get; }
        public IHandler OnLocallyFileChangeHandler { get; }
        public IHandler OnLocalyFileRenamedHandler { get; }
        public IHandler OnLocalyFileDeletedHandler { get; }
        public IHandler OnCloudFileChangeHandler { get; }
        public IHandler OnCloudFileRenamedHandler { get; }
        public IHandler OnCloudFileCreatedHandler { get; }
        public IHandler OnCloudFileDeletedHandler { get; }
    }
}
