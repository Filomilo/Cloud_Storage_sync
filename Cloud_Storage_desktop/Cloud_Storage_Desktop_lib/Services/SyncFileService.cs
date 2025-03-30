using Cloud_Storage_Desktop_lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Desktop_lib.Services
{
    class SyncFileService: IFileSyncService
    {
        public void StartSync()
        {
            throw new NotImplementedException();
        }

        public void OnRenamed(RenamedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnDeleted(FileSystemEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnCreated(FileSystemEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnChanged(FileSystemEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void PauseAllSync()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISyncProcess> GetAllSyncProcesses()
        {
            throw new NotImplementedException();
        }
    }
}
