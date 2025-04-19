using Cloud_Storage_Common.Models;

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
        LocalFileData GetFileByPathNameExtension(string path, string name, string extenstion);
        bool DoesFileExist(UploudFileData fileUpload, out LocalFileData localFileData);
    }
}
