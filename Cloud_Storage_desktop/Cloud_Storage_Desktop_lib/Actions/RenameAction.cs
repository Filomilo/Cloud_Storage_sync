using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Actions
{
    class RenameAction : AbstactAction
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("DownloadAction");

        private Action _renameAction;

        public override ActionType ActionType
        {
            get { return ActionType.RENAME_ACTION; }
        }

        public override Action ActionToRun
        {
            get { return _renameAction; }
        }

        public RenameAction(
            IServerConnection serverConnection,
            IConfiguration configuration,
            UpdateFileDataRequest update,
            IFileRepositoryService fileRepositoryService
        )
        {
            this.file = update.oldFileData.getFullFilePathForBasePath(
                configuration.StorageLocation
            );
            _renameAction = (
                () =>
                {
                    try
                    {
                        string prevPath = update.oldFileData.getFullFilePathForBasePath(
                            configuration.StorageLocation
                        );
                        string newPath = update.newFileData.getFullFilePathForBasePath(
                            configuration.StorageLocation
                        );

                        FileManager.ChangeFilePath(prevPath, newPath);
                        fileRepositoryService.UpdateFile(
                            (LocalFileData)update.oldFileData,
                            (LocalFileData)update.newFileData
                        );

                        serverConnection.UpdateFileData(update);
                    }
                    catch (Exception EX)
                    {
                        //TODO: ADD ERROR HADNLER
                        logger.LogError($"Exception while Renaming file file:: [{this.file}]");
                    }
                }
            );
        }
    }
}
