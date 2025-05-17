using System.Security.Cryptography;
using Cloud_Storage_Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Common
{
    public static class FileManager
    {
        private static ILogger Logger = CloudDriveLogging.Instance.GetLogger("FileManager");
        public const string RegexRelativePathValidation =
            "^(?:\\.|[a-zA-Z0-9_-]+(?:\\\\[a-zA-Z0-9_-]+)*)$";

        public static List<string> GetAllFilePathInLocaation(string storageLocation)
        {
            return Directory.GetFiles(storageLocation, "*.*", SearchOption.AllDirectories).ToList();
        }

        public static FileStream WaitForFile(
            string filename,
            FileMode mode,
            FileAccess acces,
            FileShare fileshare = FileShare.Read,
            int retryCount = 100,
            int delayMilliseconds = 500
        )
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return File.Open(filename, mode, acces, fileshare);
                }
                catch (IOException)
                {
                    Thread.Sleep(delayMilliseconds);
                }
            }

            throw new IOException(
                $"Unable to access file '{filename}' after {retryCount} attempts."
            );
        }

        public static List<FileData> GetAllFilesInLocation(string path)
        {
            List<FileData> listOfFIles = new List<FileData>();

            foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileinfo = new FileInfo(file);

                listOfFIles.Add(
                    new FileData()
                    {
                        Path = Path.GetDirectoryName(file),
                        Name = Path.GetFileNameWithoutExtension(file),
                        Extenstion = Path.GetExtension(file),
                    }
                );
            }

            return listOfFIles;
        }

        public static List<UploudFileData> GetUploadFileDataInLocation(string path)
        {
            List<FileData> files = GetAllFilesInLocation(path);
            List<UploudFileData> uploudFiles = new List<UploudFileData>();
            foreach (FileData fileData in files)
            {
                uploudFiles.Add(
                    FileManager.GetUploadFileData(fileData.getFullFilePathForBasePath(path), path)
                );
            }

            return uploudFiles;
        }

        public static string GetRealtivePathToFile(string filePath, string realativeTo)
        {
            string relativePath = Path.GetRelativePath(
                realativeTo,
                Path.GetDirectoryName(filePath)
            );
            relativePath = relativePath.Length == 0 ? "." : relativePath;
            return relativePath;
        }

        public static UploudFileData GetUploadFileData(string FullFilePath, string storageLocation)
        {
            return new UploudFileData()
            {
                Path = GetRealtivePathToFile(FullFilePath, storageLocation),
                Name = Path.GetFileNameWithoutExtension(FullFilePath),
                Extenstion =
                    Path.GetExtension(FullFilePath) == null ? "" : Path.GetExtension(FullFilePath),
                Hash = GetHashOfFile(FullFilePath),
                BytesSize = GetBytesSizeOfFile(FullFilePath),
            };
        }

        public static List<FileData> GetAllFilesInLocationRelative(string path)
        {
            List<FileData> listOfFIles = new List<FileData>();

            foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileinfo = new FileInfo(file);
                string relativePath = Path.GetRelativePath(path, Path.GetDirectoryName(file));
                relativePath = relativePath.Length == 0 ? "." : relativePath;
                listOfFIles.Add(
                    new FileData()
                    {
                        Path = relativePath,
                        Name = Path.GetFileNameWithoutExtension(file),
                        Extenstion = Path.GetExtension(file) == null ? "" : Path.GetExtension(file),
                    }
                );
            }

            return listOfFIles;
        }

        public static byte[] GetBytesOfFiles(string fullpath)
        {
            byte[] bytes = File.ReadAllBytes(fullpath);
            return bytes;
        }

        public static string getHashOfArrayBytes(byte[] bytes)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = new MemoryStream((bytes)))
                {
                    return Convert.ToBase64String(sha256.ComputeHash(stream));
                }
            }
        }

        public static string GetHashOfFile(string filename)
        {
            using (var sha256 = SHA256.Create())
            {
                using (
                    var stream = FileManager.WaitForFile(
                        filename,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite,
                        100,
                        5000
                    )
                )
                {
                    Logger.LogTrace($"Gettign hash for file [[{filename}]]");
                    return Convert.ToBase64String(sha256.ComputeHash(stream));
                    ;
                }
            }
        }

        public static long GetBytesSizeOfFile(string filename)
        {
            using (var stream = FileManager.WaitForFile(filename, FileMode.Open, FileAccess.Read))
            {
                Logger.LogTrace($"Gettign Byttes size for file [[{filename}]]");
                return (long)stream.Length;
                ;
            }
        }

        public static void SaveFile(string path, Stream stream)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            using (FileStream wStream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(wStream);
                File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
            }
        }

        public static FileStream GetStreamForFile(string fiePath, int ms = 500000)
        {
            Logger.LogTrace($"GetStreamForFile:: [[{fiePath}]]");
            return WaitForFile(
                fiePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite,
                100,
                ms / 100
            );
        }
        public static FileStream GetStreamForFileAsync(string fiePath, int ms = 500000)
        {
            Logger.LogTrace($"GetStreamForFile:: [[{fiePath}]]");
            return WaitForFile(
                fiePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite,
                100,
                ms / 100
            );
        }
        public static void DeleteFile(string v)
        {
            File.Delete(v);
        }

        public static void GetFilePathParamsFormRelativePath(
            string relativePath,
            out string realitveDirectory,
            out string name,
            out string extesnion
        )
        {
            name = Path.GetFileNameWithoutExtension(relativePath);
            realitveDirectory = Path.GetDirectoryName(relativePath);
            extesnion = Path.GetExtension(relativePath);
            if (realitveDirectory == "")
            {
                realitveDirectory = ".";
            }

            if (realitveDirectory == "." && name.StartsWith("."))
            {
                name = name.Substring(1);
            }
        }

        public static string GetRealtiveFullPathToFile(
            string? path,
            string configurationStorageLocation
        )
        {
            string relativePAth = GetRealtivePathToFile(path, configurationStorageLocation);
            string filename = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            return Path.Combine(relativePAth, $"{filename}{ext}");
        }

        public static void MoveFileRealitve(
            string getRealativePath,
            string s,
            IConfiguration configuration
        )
        {
            throw new NotImplementedException();
        }

        public static void ChangeFilePath(string prevPath, string newPath)
        {
            File.Move(prevPath, newPath);
        }
    }
}
