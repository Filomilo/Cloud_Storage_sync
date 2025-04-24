using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Server.Handlers;
using Cloud_Storage_Server.Interfaces;

namespace Cloud_Storage_Server.Services
{
    public class ServerChainOfResposibiltyRepository : IServerChainOfResposibiltyRepository
    {
        private IFileSystemService _fileSystemService;
        private IFileSyncService _fileSyncService;
        private IDataBaseContextGenerator _dataBaseContextGenerator;
        private IServerConfig _serverConfig;

        public ServerChainOfResposibiltyRepository(
            IFileSystemService fileSystemService,
            IFileSyncService fileSyncService,
            IDataBaseContextGenerator dataBaseContextGenerator,
            IServerConfig serverConfig
        )
        {
            _dataBaseContextGenerator = dataBaseContextGenerator;
            _fileSystemService = fileSystemService;
            _fileSyncService = fileSyncService;
            _serverConfig = serverConfig;
            OnFileAddChain = CreateOnFileAddHandler();
            OnFileUpdateChain = CreateOnFileUpdateHandler();
            OnFileRenameChain = CreateOnFileDeleteHandler();
            OnFileDeleteChain = CreateOnFileDeleteHandler();
            ChangeNewestVersionChain = CreateChangeNewestVesrionHandler();
        }

        private IHandler? CreateChangeNewestVesrionHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new UpdateNewestVersionHandler(this._dataBaseContextGenerator))
                .Next(
                    new ClearBackupsOverload(
                        this._dataBaseContextGenerator,
                        this._serverConfig,
                        this._fileSystemService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnFileDeleteHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new RemoveFileDeviceOwnership(this._dataBaseContextGenerator))
                .Next(new PrepareFileRemoveUpdateHandler())
                .Next(new SendUpdateToClientsHandler(this._fileSyncService))
                .Next(
                    new ClearBackupsOverload(
                        this._dataBaseContextGenerator,
                        this._serverConfig,
                        this._fileSystemService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnFileUpdateHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new UpdateIfOnlyOwnerChanged(this._dataBaseContextGenerator))
                .Next(new RenameIfOnlyPathChangedHandler(this._dataBaseContextGenerator))
                .Next(new SendUpdateToClientsHandler(this._fileSyncService))
                .Next(
                    new ClearBackupsOverload(
                        this._dataBaseContextGenerator,
                        this._serverConfig,
                        this._fileSystemService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnFileAddHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new SkipIfTheSameFileAlreadyExist(this._dataBaseContextGenerator))
                .Next(new UpdateIfOnlyOwnerChanged(this._dataBaseContextGenerator))
                .Next(
                    new SaveAndUpdateNewVersionOfFile(
                        this._fileSystemService,
                        _dataBaseContextGenerator
                    )
                )
                .Next(
                    new ClearBackupsOverload(
                        this._dataBaseContextGenerator,
                        this._serverConfig,
                        this._fileSystemService
                    )
                )
                .Build();
        }

        private IHandler? CreateOnFileRenameHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new UpdateIfOnlyOwnerChanged(this._dataBaseContextGenerator))
                .Next(new RenameIfOnlyPathChangedHandler(this._dataBaseContextGenerator))
                .Build();
        }

        public IHandler OnFileRenameChain { get; }
        public IHandler OnFileAddChain { get; }
        public IHandler OnFileUpdateChain { get; }
        public IHandler OnFileDeleteChain { get; }
        public IHandler ChangeNewestVersionChain { get; }
    }
}
