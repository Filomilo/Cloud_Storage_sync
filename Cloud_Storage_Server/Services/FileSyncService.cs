using System.ComponentModel.DataAnnotations;
using System.Transactions;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;

namespace Cloud_Storage_Server.Services
{
    public interface IFileSyncService
    {
        public void AddNewFile(User user, string deviceId, UploudFileData data, Stream file);
        public Stream DownloadFile(User user, SyncFileData data);
        public List<SyncFileData> ListFilesForUser(User user);
        public bool DoesFileAlreadyExist(User user, UploudFileData data);
    }

    public class FileSyncService : IFileSyncService
    {
        private IFileSystemService _fileSystemService;
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("FileSyncService");

        public FileSyncService(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        private static string GetRealtivePathForFile(User user, SyncFileData data)
        {
            //throw new NotImplementedException();
            return $"{user.id}\\{data.Id}";
        }

        public void AddNewFile(User user, string deviceId, UploudFileData data, Stream file)
        {
            SyncFileData fileData = new SyncFileData(data);
            fileData.OwnerId = user.id;
            fileData.DeviceOwner = new List<string>();
            fileData.DeviceOwner.Add(deviceId);

            try
            {
                SyncFileData saved;
                using (DatabaseContext context = new DatabaseContext())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        var validationContext = new ValidationContext(file);
                        Validator.ValidateObject(file, validationContext, true);

                        saved = context.Files.Add(fileData).Entity;
                        context.SaveChanges();

                        this._fileSystemService.SaveFile(GetRealtivePathForFile(user, saved), file);

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error saving file to server: [[]{ex.Message}]");
                throw ex;
            }
        }

        public bool DoesFileAlreadyExist(User user, UploudFileData data)
        {
            try
            {
                SyncFileData fileInRepo = FileRepository.getFileByPathNameExtensionAndUser(
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

        public Stream DownloadFile(User user, SyncFileData data)
        {
            Stream RawData = this._fileSystemService.GetFile(GetRealtivePathForFile(user, data));
            return RawData;
        }

        public List<SyncFileData> ListFilesForUser(User user)
        {
            List<SyncFileData> files = FileRepository.GetAllUserFiles(user.id);
            return files;
        }
    }
}
