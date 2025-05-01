using System.IO;
using System.IO.Pipes;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Interfaces;

namespace Cloud_Storage_Server.Services
{
    public interface IFileSystemService
    {
        public void DeleteFile(SyncFileData data);
        public Stream GetFile(string path);
        void SaveFile(SyncFileData dataBytes, Stream fileStream);
    }

    public class FileSystemService : IFileSystemService
    {
        private static ILogger Logger = CloudDriveLogging.Instance.GetLogger("FileSystemService");
        private IServerConfig _config;
        private string _basePath
        {
            get { return _config.StorageLocation; }
        }

        public FileSystemService(IServerConfig config)
        {
            this._config = config;
        }

        public void DeleteFile(SyncFileData data)
        {
            String path = GetRealtivePathForFile(data.OwnerId, data);
            File.Delete(this.GetFullPathToFile(path));
        }

        public Stream GetFile(string path)
        {
            Stream data = FileManager.GetStreamForFile(this.GetFullPathToFile(path));
            return data;
        }

        public void SaveFile(SyncFileData dataBytes, Stream fileStream)
        {
            String path = GetRealtivePathForFile(dataBytes.OwnerId, dataBytes);
            FileManager.SaveFile(GetFullPathToFile(path), fileStream);
        }

        public string GetFullPathToFile(string path)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), _basePath, path + ".dat");
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(_basePath + TrimFileName(path));
        }

        private static string TrimFileName(string pathFile)
        {
            List<string> splits = pathFile.Split(Path.DirectorySeparatorChar).ToList();
            splits.RemoveAt(splits.Count - 1);
            return Path.Combine(splits.ToArray());
        }

        internal string GetFullPathToFile(SyncFileData file)
        {
            return GetFullPathToFile(Path.Combine(file.OwnerId.ToString(), file.Id.ToString()));
        }

        private static string GetRealtivePathForFile(long userid, SyncFileData data)
        {
            return Path.Combine(userid.ToString(), data.Id.ToString());
        }
    }
}
