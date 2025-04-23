using Cloud_Storage_Common.Interfaces;

namespace Cloud_Storage_Server.Interfaces
{
    public interface IServerChainOfResposibiltyRepository
    {
        public IHandler OnFileRenameChain { get; }
        public IHandler OnFileAddChain { get; }
        public IHandler OnFileUpdateChain { get; }
        public IHandler OnFileDeleteChain { get; }
        public IHandler ChangeNewestVersionChain { get; }
    }
}
