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
            SyncFileData syncFileData
        )
        {
            this.file = syncFileData.getFullFilePathForBasePath(configuration.StorageLocation);
            _downloadAction = (
                () =>
                {
                    try
                    {
                        Stream stream = serverConnection.DownloadFile(syncFileData.Id);
                        FileManager.SaveFile(
                            syncFileData.getFullFilePathForBasePath(configuration.StorageLocation),
                            stream
                        );
                    }
                    catch (Exception EX)
                    {
                        //TODO: ADD ERROR HADNLER
                        logger.LogError($"Exception while Downloading file file:: [{this.file}]");
                    }
                }
            );
        }
    }
}
