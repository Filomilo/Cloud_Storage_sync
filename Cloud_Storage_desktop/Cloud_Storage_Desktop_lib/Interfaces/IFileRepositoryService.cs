using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface IFileRepositoryService
    {
        void AddNewFile(LocalFileData file);
        void DeleteFile(LocalFileData file);
        void UpdateFile(LocalFileData oldFileData, LocalFileData newFileData);
        IEnumerable<LocalFileData> GetAllFiles();
    }
}
