using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Actions
{
    class DeleteAction : AbstactAction
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("DeleteAction");

        private Action _deleteAction;

        public override ActionType ActionType
        {
            get { return ActionType.DELETE_ACTION; }
        }

        public override Action ActionToRun
        {
            get { return _deleteAction; }
        }

        private void DelteDirecetoryIfEmpty(string pathToDeletedFile)
        {
            string direcotry = Path.GetDirectoryName(pathToDeletedFile);
            if (!Directory.GetFiles(direcotry).Any())
            {
                Directory.Delete(direcotry);
            }
        }

        public DeleteAction(
            IServerConnection serverConnection,
            IConfiguration configuration,
            SyncFileData syncFileData,
            IFileRepositoryService fileRepositoryService
        )
        {
            this.file = syncFileData.getFullFilePathForBasePath(configuration.StorageLocation);
            _deleteAction = (
                () =>
                {
                    try
                    {
                        FileManager.DeleteFile(file);
                        DelteDirecetoryIfEmpty(file);
                        //fileRepositoryService.DeleteFileByPath(
                        //    syncFileData.Path,
                        //    syncFileData.Name,
                        //    syncFileData.Extenstion
                        //);
                    }
                    catch (Exception EX)
                    {
                        //TODO: ADD ERROR HADNLER
                        logger.LogError(
                            $"Exception while Deleteing file file:: [{this.file}] :: -- {EX.Message}"
                        );
                    }
                }
            );
        }
    }
}
