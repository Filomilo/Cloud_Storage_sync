using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Desktop_lib.SyncingHandlers
{
    class RemoveFileFromDatabaseHandler : AbstactHandler
    {
        private IConfiguration _configuration;
        private IFileRepositoryService _fileRepositoryService;
        private ILogger Logger = CloudDriveLogging.Instance.GetLogger(
            "RemoveFileFromDatabaseHandler"
        );

        public RemoveFileFromDatabaseHandler(
            IConfiguration configuration,
            IFileRepositoryService fileRepositoryService
        )
        {
            _configuration = configuration;
            _fileRepositoryService = fileRepositoryService;
        }

        public override object Handle(object request)
        {
            if (request is not string)
            {
                throw new ArgumentException(
                    "RemoveFileFromDatabaseHandler excepts argument of type string"
                );
            }

            string path = request as string;
            string relativePat = FileManager.GetRealtiveFullPathToFile(
                path,
                _configuration.StorageLocation
            );
            FileManager.GetFilePathParamsFormRelativePath(
                relativePat,
                out string directory,
                out string name,
                out string extesnion
            );
            try
            {
                this._fileRepositoryService.DeleteFileByPath(directory, name, extesnion);
            }
            catch (ArgumentException ex)
            {
                Logger.LogWarning(
                    $"{directory}{name}{extesnion} file already removed from database"
                );
            }
            if (_nextHandler != null)
            {
                _nextHandler.Handle(request);
            }

            return null;
        }
    }
}
