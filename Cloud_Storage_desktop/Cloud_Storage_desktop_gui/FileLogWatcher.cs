using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_desktop
{
    public delegate void NewLinesAdded(string lines);

    public class FileLogWatcher
    {
        private string fileLog;

        public FileLogWatcher(string Filepath)
        {
            fileLog = Filepath;
        }

        public event NewLinesAdded NewLinesAddedHandler;

        private void ActivateNewLiensAddedEevent(string text)
        {
            if (NewLinesAddedHandler != null)
            {
                NewLinesAddedHandler.Invoke(text);
            }
        }

        private FileSystemWatcher _watcher;
        private long _lastFileLength = 0;

        public void StartWatching()
        {
            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(fileLog),
                Filter = Path.GetFileName(fileLog),
                NotifyFilter = NotifyFilters.LastWrite,
            };

            _watcher.Changed += OnFileChanged;
            _watcher.EnableRaisingEvents = true;

            _lastFileLength = new FileInfo(fileLog).Length;
            string startconent = readContentFronPoint(0);
            ActivateNewLiensAddedEevent(startconent);
        }

        private string readContentFronPoint(long point)
        {
            string content = "";
            using (
                var file = File.Open(
                    this.fileLog,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                )
            )
            {
                long length = file.Length;
                long toRead = length - point;
                file.Seek(point, SeekOrigin.Begin);
                content = new StreamReader(file).ReadToEnd();
            }

            return content;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            string newContnet = readContentFronPoint(_lastFileLength);
            _lastFileLength += newContnet.Length;
            ActivateNewLiensAddedEevent(newContnet);
        }
    }
}
