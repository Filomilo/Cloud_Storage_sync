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

        static Mutex DirecotrYDeleteMutex= new Mutex();

        private void DelteDirecetoryIfEmpty(IConfiguration configuration, string pathToDeletedFile)
        {

            //sometimes casued deeltion of sync foldfer     

            //DirecotrYDeleteMutex.WaitOne();
            //string direcotry = Path.GetDirectoryName(pathToDeletedFile);
            //if (
            //    configuration.StorageLocation.Equals(direcotry + "\\")
            //    || configuration.StorageLocation.Equals(direcotry)
            //)
            //{
            //    return;
            //}
            //if (!Directory.GetFiles(direcotry).Any())
            //{
            //    Directory.Delete(direcotry);
            //}
            //DirecotrYDeleteMutex.ReleaseMutex();
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
                        DelteDirecetoryIfEmpty(configuration, file);
                        fileRepositoryService.DeleteFileByPath(
                            syncFileData.Path,
                            syncFileData.Name,
                            syncFileData.Extenstion
                        );
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
