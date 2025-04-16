using Cloud_Storage_Common.Interfaces;

namespace Cloud_Storage_Server.Interfaces
{
    public interface IServerChainOfResposibiltyRepository
    {
        public IHandler OnFileRenameHandler { get; }
        public IHandler OnFileAddHandler { get; }
        public IHandler OnFileUpdateHandler { get; }
        public IHandler OnFileDeleteHandler { get; }
    }
}
