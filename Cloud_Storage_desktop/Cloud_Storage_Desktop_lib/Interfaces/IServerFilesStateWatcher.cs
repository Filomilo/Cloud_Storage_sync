using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public delegate void OnSyncFileChanged(
        IEnumerable<SyncFileData> updatedFiles,
        IEnumerable<SyncFileData> addFiles,
        IEnumerable<SyncFileData> removedFiles
    );

    public interface IServerFilesStateWatcher
    {
        IEnumerable<SyncFileData> SyncFileData { get; }
        void RefreshList();
        event OnSyncFileChanged SyncFileChangedEvent;
    }
}
