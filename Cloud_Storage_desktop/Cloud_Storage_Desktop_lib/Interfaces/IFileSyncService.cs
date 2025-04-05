using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface ISyncProcess { }

    public interface ISyncProcessEventArgs { }

    public interface IFileSyncService
    {
        bool Active { get; }

        void StartSync();
        void OnLocallyOnRenamed(RenamedEventArgs args);
        void OnLocallyDeleted(FileSystemEventArgs args);
        void OnLocallyCreated(FileSystemEventArgs args);
        void OnLocallyChanged(FileSystemEventArgs args);

        void StopAllSync();

        IEnumerable<ISyncProcess> GetAllSyncProcesses();

        delegate void OnSyncProcessUpdate(ISyncProcessEventArgs args);
    }
}
