using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Actions
{
    public class DownloadAction : AbstactAction
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("DownloadAction");

        private Action _downloadAction;

        public override ActionType ActionType
        {
            get { return ActionType.DOWNLOAD_ACTION; }
        }

        public override Action ActionToRun
        {
            get { return _downloadAction; }
        }

        public DownloadAction(
            IServerConnection serverConnection,
            IConfiguration configuration,
            SyncFileData syncFileData,
            IFileRepositoryService fileRepositoryService
        )
        {
            this.file = syncFileData.getFullFilePathForBasePath(configuration.StorageLocation);
            _downloadAction = (
                () =>
                {
                    try
                    {
                        // update file data
                        LocalFileData newData = (LocalFileData)syncFileData;
                        LocalFileData oldData = fileRepositoryService
                            .GetAllFiles()
                            .FirstOrDefault(x =>
                                x.GetRealativePath().Equals(syncFileData.GetRealativePath())
                            );
                        if (oldData != null)
                            fileRepositoryService.UpdateFile(oldData, newData);
                        else
                        {
                            fileRepositoryService.AddNewFile(newData);
                        }
                        Stream stream = serverConnection.DownloadFile(syncFileData.Id);
                        FileManager.SaveFile(
                            syncFileData.getFullFilePathForBasePath(configuration.StorageLocation),
                            stream
                        );
                        serverConnection.UpdateFileData(
                            new UpdateFileDataRequest(UPDATE_TYPE.ADD, null, newData)
                        );
                    }
                    catch (Exception EX)
                    {
                        logger.LogError(
                            $"Exception while Downloading file :: [{this.file}] ::: [[{EX.Message}]]"
                        );
                    }
                }
            );
        }
    }
}
