using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Desktop_lib.Interfaces
{
    public interface IFileRepositoryService
    {
        AbstractDataBaseContext GetDbContext();
        void AddNewFile(LocalFileData file);
        void DeleteFile(LocalFileData file);
        void UpdateFile(LocalFileData oldFileData, LocalFileData newFileData);
        IEnumerable<LocalFileData> GetAllFiles();
        void Reset();
        void DeleteFileByPath(string realitveDirectory, string name, string extesnion);
    }
}
