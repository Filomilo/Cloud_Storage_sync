using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Common
{
    public static class FileManager
    {
        public const string RegexRelativePathValidation =
            "^(?:\\.|[a-zA-Z0-9_-]+(?:\\\\[a-zA-Z0-9_-]+)*)$";

        public static List<string> GetAllFilePathInLocaation(string storageLocation)
        {
            return Directory.GetFiles(storageLocation, "*.*", SearchOption.AllDirectories).ToList();
        }

        public static List<UploudFileData> GetAllFilesInLocation(string path)
        {
            List<UploudFileData> listOfFIles = new List<UploudFileData>();

            foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileinfo = new FileInfo(file);

                listOfFIles.Add(
                    new UploudFileData()
                    {
                        Path = Path.GetDirectoryName(file),
                        Name = Path.GetFileNameWithoutExtension(file),
                        Extenstion = Path.GetExtension(file),
                        Hash = GetHashOfFile(file),
                    }
                );
            }

            return listOfFIles;
        }

        public static string GetRealtivePathToFile(string filePath, string realativeTo)
        {
            string relativePath = Path.GetRelativePath(realativeTo, Path.GetDirectoryName(filePath));
            relativePath = relativePath.Length == 0 ? "." : relativePath;
            return relativePath;
        }
        public static UploudFileData GetUploadFileData(string filePath,string storageLocation)
        {
           
            return new UploudFileData()
            {
                Path = GetRealtivePathToFile(filePath,storageLocation),
                Name = Path.GetFileNameWithoutExtension(filePath),
                Extenstion = Path.GetExtension(filePath) == null ? "" : Path.GetExtension(filePath),
                Hash = GetHashOfFile(filePath)
            };
        }
        public static List<UploudFileData> GetAllFilesInLocationRelative(string path)
        {
            List<UploudFileData> listOfFIles = new List<UploudFileData>();

            foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileinfo = new FileInfo(file);
                string relativePath = Path.GetRelativePath(path, Path.GetDirectoryName(file));
                relativePath = relativePath.Length == 0 ? "." : relativePath;
                listOfFIles.Add(
                    new UploudFileData()
                    {
                        Path = relativePath,
                        Name = Path.GetFileNameWithoutExtension(file),
                        Extenstion = Path.GetExtension(file) == null ? "" : Path.GetExtension(file),
                        Hash = GetHashOfFile(file),
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
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream((bytes)))
                {
                    return Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
        }

     
        public static string GetHashOfFile(string filename)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", ""); ;
                }
            }
        }

        public static void SaveFile(string path, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (DirectoryNotFoundException ex)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, bytes);
            }
            File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
        }
    }
}
