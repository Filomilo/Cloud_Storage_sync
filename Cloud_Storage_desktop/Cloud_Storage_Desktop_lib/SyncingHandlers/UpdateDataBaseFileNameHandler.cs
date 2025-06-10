using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    public class UpdateDataBaseFileNameHandler : AbstactHandler
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger(
            "UpdateDataBaseFileNameHandler"
        );
        IFileRepositoryService _fileRepositoryService;
        IConfiguration _configuration;

        public UpdateDataBaseFileNameHandler(
            IFileRepositoryService repository,
            IConfiguration configuration
        )
        {
            _fileRepositoryService = repository;
            _configuration = configuration;
        }

        public override object Handle(object request)
        {
            if (request is not RenamedEventArgs)
                throw new ArgumentException(
                    "ValidateIfFileAlreadyExisitInDataBase excepts argument of type SyncFileData"
                );

            try
            {
                RenamedEventArgs renamedEventArgs = request as RenamedEventArgs;
                string oldPath = renamedEventArgs.OldFullPath;
                string newPath = renamedEventArgs.FullPath;

                FileManager.GetFilePathParamsFormRelativePath(
                    FileManager.GetRealtiveFullPathToFile(oldPath, _configuration.StorageLocation),
                    out string oldDir,
                    out string oldName,
                    out string oldExtesnion
                );

                FileManager.GetFilePathParamsFormRelativePath(
                    FileManager.GetRealtiveFullPathToFile(newPath, _configuration.StorageLocation),
                    out string newDir,
                    out string newName,
                    out string newExtesnion
                );
                LocalFileData newFileData = null;
                LocalFileData oldFileDataCopy = null;
                using (var ctx = this._fileRepositoryService.GetDbContext())
                {
                    using (var transaction = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            LocalFileData oldFileData = ctx
                                .Files.Where(x =>
                                    x.Path.Equals(oldDir)
                                    && x.Name.Equals(oldName)
                                    && x.Extenstion.Equals(oldExtesnion)
                                )
                                .FirstOrDefault();

                            oldFileDataCopy = oldFileData == null ? null : oldFileData.Clone();

                            if (oldFileData != null)
                            {
                                if (!oldFileData.ComparePath(newDir, newName, newExtesnion))
                                {
                                    ctx.Files.Remove(oldFileData);
                                    LocalFileData cloned = oldFileData.Clone();
                                    ctx.SaveChanges();

                                    cloned.Name = newName;
                                    cloned.Extenstion = newExtesnion;
                                    cloned.Path = newDir;
                                    cloned.Version++;
                                    ctx.Files.Add(cloned);
                                    newFileData = cloned;
                                }
                            }

                            ctx.SaveChanges();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            logger.LogError(
                                $"Error while starting transaction in UpdateDataBaseFileNameHandler: {ex.Message}::\n {ex.StackTrace}"
                            );
                            throw ex;
                        }
                    }
                }

                UpdateFileDataRequest updateFileDataRequest = new UpdateFileDataRequest()
                {
                    oldFileData =
                        oldFileDataCopy == null ? null : new SyncFileData(oldFileDataCopy),
                    newFileData = newFileData == null ? null : new SyncFileData(newFileData),
                };

                if (
                    this._nextHandler != null
                    && updateFileDataRequest != null
                    && updateFileDataRequest.newFileData != null
                )
                {
                    return this._nextHandler.Handle(updateFileDataRequest);
                }

                return updateFileDataRequest;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"Error while handlign rename hadnler: {ex.Message}::\n {ex.StackTrace}"
                );
                throw ex;
            }
        }
    }
}
