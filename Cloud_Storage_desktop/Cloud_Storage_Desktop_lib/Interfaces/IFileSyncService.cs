using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Desktop_lib.Interfaces
{

    public interface ISyncProcess
    {

    }

    public interface ISyncProcessEventArgs
    {

    }

    public interface IFileSyncService
    {
        void StartSync();
        void OnRenamed(RenamedEventArgs args);
        void OnDeleted(FileSystemEventArgs args);
        void OnCreated(FileSystemEventArgs args);
        void OnChanged(FileSystemEventArgs args);

        void PauseAllSync();

        IEnumerable<ISyncProcess> GetAllSyncProcesses();

        delegate void OnSyncProcessUpdate(ISyncProcessEventArgs args);

    }
}
