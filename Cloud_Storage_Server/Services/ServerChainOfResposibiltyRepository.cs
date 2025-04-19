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

        public ServerChainOfResposibiltyRepository(
            IFileSystemService fileSystemService,
            IFileSyncService fileSyncService,
            IDataBaseContextGenerator dataBaseContextGenerator
        )
        {
            _dataBaseContextGenerator = dataBaseContextGenerator;
            _fileSystemService = fileSystemService;
            _fileSyncService = fileSyncService;
            OnFileAddHandler = CreateOnFileAddHandler();
            OnFileUpdateHandler = CreateOnFileUpdateHandler();
            OnFileRenameHandler = CreateOnFileDeleteHandler();
            OnFileDeleteHandler = CreateOnFileDeleteHandler();
        }

        private IHandler? CreateOnFileDeleteHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new RemoveFileDeviceOwnership(this._dataBaseContextGenerator))
                .Next(new PrepareFileRemoveUpdateHandler())
                .Next(new SendUpdateToClientsHandler(this._fileSyncService))
                .Build();
        }

        private IHandler? CreateOnFileUpdateHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new UpdateIfOnlyOwnerChanged(this._dataBaseContextGenerator))
                .Next(new RenameIfOnlyPathChangedHandler(this._dataBaseContextGenerator))
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
                .Build();
        }

        private IHandler? CreateOnFileRenameHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new UpdateIfOnlyOwnerChanged(this._dataBaseContextGenerator))
                .Next(new RenameIfOnlyPathChangedHandler(this._dataBaseContextGenerator))
                .Build();
        }

        public IHandler OnFileRenameHandler { get; }
        public IHandler OnFileAddHandler { get; }
        public IHandler OnFileUpdateHandler { get; }
        public IHandler OnFileDeleteHandler { get; }
    }
}
