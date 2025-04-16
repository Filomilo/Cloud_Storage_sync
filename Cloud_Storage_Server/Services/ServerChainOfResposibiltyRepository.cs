using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Server.Handlers;
using Cloud_Storage_Server.Interfaces;

namespace Cloud_Storage_Server.Services
{
    public class ServerChainOfResposibiltyRepository : IServerChainOfResposibiltyRepository
    {
        private IFileSystemService _fileSystemService;

        public ServerChainOfResposibiltyRepository(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;

            OnFileRenameHandler = CreateOnFileRenameHandler();
            OnFileAddHandler = CreateOnFileAddHandler();
            OnFileUpdateHandler = CreateOnFileUpdateHandler();
            OnFileRenameHandler = CreateOnFileDeleteHandler();
            OnFileDeleteHandler = CreateOnFileDeleteHandler();
        }

        private IHandler? CreateOnFileDeleteHandler()
        {
            return new ChainOfResponsiblityBuilder().Next(new RemoveFileDeviceOwnership()).Build();
        }

        private IHandler? CreateOnFileUpdateHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new UpdateIfOnlyOwnerChanged())
                .Next(new RenameIfOnlyPathChangedHandler())
                .Build();
        }

        private IHandler? CreateOnFileAddHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new SkipIfTheSameFileAlreadyExist())
                .Next(new UpdateIfOnlyOwnerChanged())
                .Next(new SaveAndUpdateNewVersionOfFile(this._fileSystemService))
                .Build();
        }

        private IHandler? CreateOnFileRenameHandler()
        {
            return new ChainOfResponsiblityBuilder()
                .Next(new UpdateIfOnlyOwnerChanged())
                .Next(new RenameIfOnlyPathChangedHandler())
                .Build();
        }

        public IHandler OnFileRenameHandler { get; }
        public IHandler OnFileAddHandler { get; }
        public IHandler OnFileUpdateHandler { get; }
        public IHandler OnFileDeleteHandler { get; }
    }
}
