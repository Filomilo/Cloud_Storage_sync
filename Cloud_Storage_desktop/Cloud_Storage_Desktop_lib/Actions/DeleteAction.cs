using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
