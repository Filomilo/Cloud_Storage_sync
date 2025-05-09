using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_desktop
{
    public delegate void NewLinesAdded(string lines);

    public class FileLogWatcher
    {
        private string fileLog;
        private bool hasChanges = false;
        private object locker = new object();
        private static System.Timers.Timer _timer;

        public FileLogWatcher(string Filepath)
        {
            fileLog = Filepath;

            //task = new Start(() =>
            //{
            //    while (true)
            //    {
            //        if (hasChanges)
            //        {
            //            lock (locker)
            //            {
            //                fileChangeHadnler();
            //                this.hasChanges = false;
            //            }
            //        }

            //        Thread.Sleep(100);
            //    }
            //});
        }

        void readThreadTask()
        {
            _timer.Stop();
            fileChangeHadnler();
            _timer.Interval = 1000;
            _timer.Start();
        }

        public void fileChangeHadnler()
        {
            string newContnet = readContentFronPoint(_lastFileLength);
            _lastFileLength += newContnet.Length;
            ActivateNewLiensAddedEevent(newContnet);
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
            //_watcher = new FileSystemWatcher
            //{
            //    Path = Path.GetDirectoryName(fileLog),
            //    Filter = Path.GetFileName(fileLog),
            //    NotifyFilter = NotifyFilters.LastWrite,
            //};


            _lastFileLength = new FileInfo(fileLog).Length;
            string startconent = readContentFronPoint(0);
            ActivateNewLiensAddedEevent(startconent);

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (
                (sender, args) =>
                {
                    readThreadTask();
                }
            );

            _timer.Enabled = true;

            //_watcher.Changed += OnFileChanged;
        }

        //private void OnFileChanged(object sender, FileSystemEventArgs e)
        //{
        //    this.hasChanges = true;
        //}

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
                char[] buffer = new char[toRead];
                new StreamReader(file).Read(buffer);
                content = new string(buffer);
            }

            return content;
        }
    }
}
