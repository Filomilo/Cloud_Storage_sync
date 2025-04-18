using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Interfaces;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    interface IClientChainOfResponsibilityRepository
    {
        IHandler InitlalSyncHandler { get; }
        IHandler OnLocallyFileChangeHandler { get; }
        IHandler OnLocalyFileRenamedHandler { get; }
        IHandler OnLocalyFileDeletedHandler { get; }

        IHandler OnCloudFileChangeHandler { get; }
        IHandler OnCloudFileRenamedHandler { get; }
        IHandler OnCloudFileCreatedHandler { get; }
        IHandler OnCloudFileDeletedHandler { get; }
    }
}
