using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;

namespace Cloud_Storage_Server.Services
{
    public interface IFileSyncService
    {
        public void AddNewFile(User user, UploudFileData data, byte[] file);
        public byte[] DownloadFile(User user, FileData data);
        public List<FileData> ListFilesForUser(User user);
        public bool DoesFileAlreadyExist(User user, UploudFileData data);
    }

    public class FileSyncService : IFileSyncService
    {
        private IFileSystemService _fileSystemService;

        public FileSyncService(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        private static string GetRealtivePathForFile(User user, FileData data)
        {
            return $"{user.id}\\{data.Id}";
        }

        public void AddNewFile(User user, UploudFileData data, byte[] file)
        {
            FileData fileData = new FileData(data);
            fileData.OwnerId = user.id;
            FileData saved = FileRepository.SaveNewFile(fileData);

            this._fileSystemService.SaveFile(GetRealtivePathForFile(user, saved), file);
            //todo: save actual file
        }

        public bool DoesFileAlreadyExist(User user, UploudFileData data)
        {
            try
            {
                FileData fileInRepo = FileRepository.getFileByPathNameExtensionAndUser(
                    data.Path,
                    data.Name,
                    data.Extenstion,
                    user.id
                );
                if (fileInRepo == null || fileInRepo.Hash == data.Hash)
                {
                    return true;
                }
            }
            catch (KeyNotFoundException ex)
            {
                return false;
            }
            return false;
        }

        public byte[] DownloadFile(User user, FileData data)
        {
            byte[] RawData = this._fileSystemService.GetFile(GetRealtivePathForFile(user, data));
            return RawData;
        }

        public List<FileData> ListFilesForUser(User user)
        {
            List<FileData> files = FileRepository.GetAllUserFiles(user.id);
            return files;
        }
    }
}
