using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class FileSystemWatcher : IFIleSystemWatcher
    {
        private System.IO.FileSystemWatcher _Watcher;
        private string _Path;

        public FileSystemWatcher() { }

        public string Directory
        {
            get { return _Path; }
            set
            {
                if (_Watcher != null)
                {
                    _Watcher.Dispose();
                }
                _Watcher = new System.IO.FileSystemWatcher(value);

                _Watcher.NotifyFilter =
                    NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size;

                _Watcher.Changed += _OnChanged;
                _Watcher.Created += _OnCreated;
                _Watcher.Deleted += _OnDeleted;
                _Watcher.Renamed += _OnRenamed;
                _Watcher.Error += _OnError;
                _Watcher.IncludeSubdirectories = true;
                _Watcher.EnableRaisingEvents = true;

                _Path = value;
            }
        }

        private void _OnChanged(object sender, FileSystemEventArgs e)
        {
            if (this.OnChangedEventHandler != null)
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                    this.OnChangedEventHandler.Invoke(e);
            }
        }

        private void _OnCreated(object sender, FileSystemEventArgs e)
        {
            if (this.OnCreatedEventHandler != null)
            {
                this.OnCreatedEventHandler.Invoke(e);
            }
        }

        private void _OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (this.OnDeletedEventHandler != null)
            {
                this.OnDeletedEventHandler.Invoke(e);
            }
        }

        private void _OnRenamed(object sender, RenamedEventArgs e)
        {
            if (this.OnRenamedEventHandler != null)
            {
                this.OnRenamedEventHandler.Invoke(e);
            }
        }

        private void _OnError(object sender, ErrorEventArgs e)
        {
            if (this.OnErrorEventHandler != null)
            {
                this.OnErrorEventHandler.Invoke(e);
            }
        }

        public event OnError OnErrorEventHandler;
        public event OnRenamed OnRenamedEventHandler;
        public event OnDeleted OnDeletedEventHandler;
        public event OnCreated OnCreatedEventHandler;
        public event OnChanged OnChangedEventHandler;

        public void Stop()
        {
            this._Watcher.Dispose();
        }

        public void Dispose()
        {
            this._Watcher.Dispose();
        }
    }
}
