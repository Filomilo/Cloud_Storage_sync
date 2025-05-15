using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_desktop
{
    /// <summary>
    /// Interaction logic for CloudFilesList.xaml
    /// </summary>
    public partial class CloudFilesList : UserControl
    {
        private class GuiDataElement
        {
            private SyncFileData _syncFileData;

            public GuiDataElement(SyncFileData syncFileData)
            {
                this._syncFileData = syncFileData;
            }

            public String Path
            {
                get { return this._syncFileData.GetRealativePath(); }
            }
            public ulong Version
            {
                get { return this._syncFileData.Version; }
            }
            public long BytesSize
            {
                get { return this._syncFileData.BytesSize; }
            }

            public Guid guid
            {
                get { return this._syncFileData.Id; }
            }
        }

        ObservableCollection<GuiDataElement> _bservableCollection =
            new ObservableCollection<GuiDataElement>();
        private IServerFilesStateWatcher _serverFilesStateWatcher;
        private IServerConnection _serverConnection;

        public void Setup(
            IServerFilesStateWatcher filesStateWatcher,
            IServerConnection serverConnection
        )
        {
            Dispatcher.Invoke(() =>
            {
                this._bservableCollection.Clear();
            });

            _serverFilesStateWatcher = filesStateWatcher;
            _serverFilesStateWatcher.SyncFileChangedEvent +=
                ServerFilesStateWatcherOnSyncFileChangedEvent;
            _serverConnection = serverConnection;

            _serverFilesStateWatcher.RefreshList();
        }

        public CloudFilesList()
        {
            InitializeComponent();
        }

        void ServerFilesStateWatcherOnSyncFileChangedEvent(
            IEnumerable<SyncFileData> updatedfiles,
            IEnumerable<SyncFileData> addfiles,
            IEnumerable<SyncFileData> removedfiles
        )
        {
            Dispatcher.BeginInvoke(() =>
            {
                foreach (SyncFileData addedFile in addfiles)
                {
                    _bservableCollection.Add(new GuiDataElement(addedFile));
                }

                foreach (SyncFileData removedElement in removedfiles)
                {
                    _bservableCollection.Remove(
                        _bservableCollection.First(x =>
                            x.Path == removedElement.GetRealativePath()
                            && x.Version == removedElement.Version
                        )
                    );
                }
            });
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void ListView_files_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void CloudFilesList_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataGrid_Files.ItemsSource = _bservableCollection;

            //SyncFileData syncFileData = new SyncFileData()
            //{
            //    BytesSize = 1024,
            //    DeviceOwner = new List<string>() { "asd", "dsa" },
            //    Extenstion = "pn4",
            //    Hash = "123560",
            //    Id = Guid.NewGuid(),
            //    Name = "name",
            //    OwnerId = 3,
            //    OwnersDevices = new List<Device>() { new Device(), new Device() },
            //    Path = "D:/asd/asd/asdas/das/",
            //};
            //SyncFileData syncFileDataNewest = syncFileData.Clone();
            //syncFileDataNewest.Version = syncFileData.Version + 1;

            //GuiDataElement guiDataElement = new GuiDataElement(syncFileData);
            //GuiDataElement guiDataElementnewer = new GuiDataElement(syncFileDataNewest);

            //_bservableCollection.Add(guiDataElement);
            //_bservableCollection.Add(guiDataElement);
            //_bservableCollection.Add(guiDataElementnewer);
            //_bservableCollection.Add(guiDataElement);
            //_bservableCollection
        }

        private ulong GetNewestVersionOfFileWIhtRealitvePath(string realitvePath)
        {
            GuiDataElement guiDataElement = _bservableCollection
                .Where(x => x.Path == realitvePath)
                .OrderBy(x => x.Version)
                .FirstOrDefault();
            return guiDataElement == null ? 0 : guiDataElement.Version;
        }

        private GuiDataElement selected = null;

        private void DataGrid_Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool canRestore = false;
            if (
                DataGrid_Files.CurrentCell != null
                && DataGrid_Files.CurrentCell.Item is GuiDataElement
            )
            {
                GuiDataElement guiDataElement = DataGrid_Files.CurrentCell.Item as GuiDataElement;
                if (guiDataElement != null)
                {
                    selected = guiDataElement;
                    String path = guiDataElement.Path;
                    ulong newestVersion = GetNewestVersionOfFileWIhtRealitvePath(path);
                    if (guiDataElement.Version == newestVersion)
                    {
                        canRestore = true;
                    }
                }
            }

            Button_Restore.IsEnabled = canRestore;
        }

        private void Button_Restore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selected != null)
                {
                    this._serverConnection.SetFileVersion(selected.guid, selected.Version);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    exception.Message,
                    "Error restoring file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
