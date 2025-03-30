using System.IO;
using Cloud_Storage_Common;
using Cloud_Storage_Server.Database.Models;

namespace Cloud_Storage_Server.Services
{
    public interface IFileSystemService
    {
        public void SaveFile(string path, Stream dataBytes);
        public void DeleteFile(string path);
        public Stream GetFile(string path);
    }

    public class FileSystemService : IFileSystemService
    {
        private string _basePath;

        public FileSystemService(string path)
        {
            this._basePath = path;
        }

        public void DeleteFile(string path)
        {
            File.Delete(this.GetFullPathToFile(path));
        }

        public Stream GetFile(string path)
        {
            Stream data = FileManager.GetStreamForFile(this.GetFullPathToFile(path));
            return data;
        }

        private string GetFullPathToFile(string path)
        {
            return Directory.GetCurrentDirectory() + "\\" + _basePath + path + ".dat";
        }

        public void SaveFile(string path, Stream stream)
        {
            FileManager.SaveFile(GetFullPathToFile(path), stream);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(_basePath + TrimFileName(path));
        }

        private static string TrimFileName(string parhFile)
        {
            List<string> splits = parhFile.Split("\\").ToList();
            splits.RemoveAt(splits.Count - 1);
            return string.Join("\\", splits);
        }
    }
}
