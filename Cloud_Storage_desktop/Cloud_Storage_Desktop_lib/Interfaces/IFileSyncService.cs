namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface ISyncProcess { }

    public interface ISyncProcessEventArgs { }

    public enum SyncState
    {
        NOT_SETUP,
        DISCONNECTED,
        CONNECTED,
        PAUSED,
        STOPPED,
    }

    public interface IFileSyncService
    {
        bool Active { get; }

        void StartSync();
        void OnLocallyOnRenamed(RenamedEventArgs args);
        void OnLocallyDeleted(FileSystemEventArgs args);
        void OnLocallyChanged(FileSystemEventArgs args);

        void StopAllSync();
        void PauseAllSync();

        IEnumerable<ISyncProcess> GetAllSyncProcesses();
        void ResumeAllSync();
        void Dispose();

        delegate void OnSyncProcessUpdate(ISyncProcessEventArgs args);

        SyncState State { get; }
        void ResetSync();
    }
}
