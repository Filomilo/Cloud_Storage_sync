using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class FileSystemWatcher : IFIleSystemWatcher
    {
        private ILogger _logger = CloudDriveLogging.Instance.GetLogger("FileSystemWatcher");
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
                _Watcher.Deleted += _OnDeleted;
                _Watcher.Renamed += _OnRenamed;
                _Watcher.Error += _OnError;
                _Watcher.Created += OnCreated;
                _Watcher.IncludeSubdirectories = true;
                _Watcher.EnableRaisingEvents = true;

                _Path = value;
            }
        }

        private static ulong onChangeCounterTmp = 0;

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
                return;
            if (this.OnChangedEventHandler != null)
            {
                if (e.ChangeType == WatcherChangeTypes.Created)
                    this.OnChangedEventHandler.Invoke(e);
            }
        }

        private void _OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                return;
            }
            if (!File.Exists(e.FullPath))
                return;
            onChangeCounterTmp++;
            _logger.LogDebug($"_OnChanged:: {e.FullPath} :::: {onChangeCounterTmp}");
            if (this.OnChangedEventHandler != null)
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                    this.OnChangedEventHandler.Invoke(e);
            }
        }

        private void _OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (this.OnDeletedEventHandler != null)
            {
                try
                {
                    this.OnDeletedEventHandler.Invoke(e);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"failed to handle file deletion: {ex.Message}");
                }
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
        public event OnChanged OnChangedEventHandler;

        public void Stop()
        {
            if (this._Watcher != null)
                this._Watcher.Dispose();
        }

        public void Dispose()
        {
            if (this._Watcher != null)
                this._Watcher.Dispose();
        }
    }
}
