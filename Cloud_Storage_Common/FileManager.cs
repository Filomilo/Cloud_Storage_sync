using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Common
{
    public static class FileManager
    {
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
                        SyncDate = DateTime.Now,
                    }
                );
            }

            return listOfFIles;
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
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
        }
    }
}
