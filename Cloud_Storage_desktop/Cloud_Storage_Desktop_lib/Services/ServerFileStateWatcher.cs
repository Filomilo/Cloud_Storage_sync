using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class ServerFileStateWatcher : IServerFilesStateWatcher
    {
        IServerConnection _connection;

        public ServerFileStateWatcher(IServerConnection serverConnection)
        {
            _connection = serverConnection;
        }

        public List<SyncFileData> _SyncFileData = new List<SyncFileData>();

        public IEnumerable<SyncFileData> SyncFileData
        {
            get { return _SyncFileData; }
        }

        private List<SyncFileData> GetRemvoedFiles(List<SyncFileData> newData)
        {
            return _SyncFileData.Where(x => newData.Contains(x) == false).ToList();
        }

        private List<SyncFileData> GetAddedFiles(List<SyncFileData> newData)
        {
            return newData.Where(x => _SyncFileData.Contains(x) == false).ToList();
        }

        public void RefreshList()
        {
            List<SyncFileData> Addlist = new List<SyncFileData>();
            List<SyncFileData> Updatelist = new List<SyncFileData>();
            List<SyncFileData> Deletelist = new List<SyncFileData>();
            if (_connection.CheckIfAuthirized() == false)
            {
                Deletelist = this.SyncFileData.ToList();
                activateOnUpdateEvent(Addlist, Updatelist, Deletelist);
                return;
            }

            List<SyncFileData> newFiles = _connection.GetListOfFiles();
            Addlist = GetAddedFiles(newFiles);
            Deletelist = GetRemvoedFiles(newFiles);
            _SyncFileData = newFiles;
            activateOnUpdateEvent(Addlist, Updatelist, Deletelist);
        }

        private void activateOnUpdateEvent(
            List<SyncFileData> Addlist,
            List<SyncFileData> Updatelist,
            List<SyncFileData> Deletelist
        )
        {
            if (SyncFileChangedEvent != null)
            {
                SyncFileChangedEvent.Invoke(Updatelist, Addlist, Deletelist);
            }
        }

        public event OnSyncFileChanged? SyncFileChangedEvent;
    }
}
