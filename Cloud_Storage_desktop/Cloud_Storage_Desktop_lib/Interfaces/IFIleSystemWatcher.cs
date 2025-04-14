using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public delegate void OnChanged(FileSystemEventArgs e);

    public delegate void OnCreated(FileSystemEventArgs e);

    public delegate void OnDeleted(FileSystemEventArgs e);

    public delegate void OnRenamed(RenamedEventArgs e);

    public delegate void OnError(ErrorEventArgs e);

    public interface IFIleSystemWatcher
    {
        string Directory { get; set; }
        event OnError OnErrorEventHandler;
        event OnRenamed OnRenamedEventHandler;
        event OnDeleted OnDeletedEventHandler;
        event OnCreated OnCreatedEventHandler;
        event OnChanged OnChangedEventHandler;
        void Stop();
        void Dispose();
    }
}
