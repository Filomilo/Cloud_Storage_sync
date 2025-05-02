using Cloud_Storage_Common.Interfaces;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    interface IClientChainOfResponsibilityRepository
    {
        IHandler InitlalLocalySyncHandler { get; }
        IHandler InitlalConnectedSyncHandler { get; }
        IHandler OnLocallyFileChangeHandler { get; }
        IHandler OnLocalyFileRenamedHandler { get; }
        IHandler OnLocalyFileDeletedHandler { get; }

        IHandler OnCloudFileChangeHandler { get; }
        IHandler OnCloudFileRenamedHandler { get; }
        IHandler OnCloudFileCreatedHandler { get; }
        IHandler OnCloudFileDeletedHandler { get; }
    }
}
