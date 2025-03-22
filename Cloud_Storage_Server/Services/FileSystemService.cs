using Cloud_Storage_Server.Database.Models;
using System.IO;

namespace Cloud_Storage_Server.Services
{
    public interface IFileSystemService
    {
        public void SaveFile(string path, byte[] dataBytes);
        public void DeleteFile(string path);
        public byte[] GetFile(string path);
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

        public byte[] GetFile(string path)
        {
            byte[] data = File.ReadAllBytes(this.GetFullPathToFile(path));
            return data;
        }

        private string GetFullPathToFile(string path)
        {
            return _basePath + path;
        }
        public void SaveFile(string path, byte[] dataBytes)
        {
            
            try
            {
                File.WriteAllBytes(GetFullPathToFile(path), dataBytes);

            }
            catch (DirectoryNotFoundException ex)
            {
                this.CreateDirectory(path);
                File.WriteAllBytes(_basePath + path, dataBytes);
            }
            File.SetAttributes(GetFullPathToFile(path), File.GetAttributes(GetFullPathToFile(path)) & ~FileAttributes.ReadOnly);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(_basePath+ TrimFileName(path));
        }

        private static string TrimFileName(string parhFile)
        {
            List<string> splits = parhFile.Split("\\").ToList();
            splits.RemoveAt(splits.Count - 1);
            return string.Join("\\",splits);
        }
    }

}
