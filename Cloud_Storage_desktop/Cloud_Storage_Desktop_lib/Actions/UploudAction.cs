using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.Actions
{
    public class UploadAction : AbstactAction
    {
        private ILogger logger = CloudDriveLogging.Instance.GetLogger("UploadAction");

        private Action _uploudAction;

        public override ActionType ActionType
        {
            get { return ActionType.UPLOAD_ACTION; }
        }

        public override Action ActionToRun
        {
            get { return _uploudAction; }
        }

        public UploadAction(
            IServerConnection serverConnection,
            IConfiguration configuration,
            IFileRepositoryService fileRepositoryService,
            UploudFileData fileData
        )
        {
            this.file = fileData.getFullFilePathForBasePath(configuration.StorageLocation);
            _uploudAction = (
                () =>
                {
                    try
                    {
                        using (
                            Stream stream = FileManager.GetStreamForFile(
                                fileData.getFullFilePathForBasePath(configuration.StorageLocation)
                            )
                        )
                        {
                            //fileRepositoryService.AddNewFile(new LocalFileData(fileData));
                            serverConnection.UploudFile(fileData, stream);
                        }
                    }
                    catch (Exception EX)
                    {
                        //TODO: ADD ERROR HADNLER
                        logger.LogError(
                            $"Exception while uplound file:: [{this.file}] [[{EX.Message}]]"
                        );
                    }
                }
            );
        }
    }
}
