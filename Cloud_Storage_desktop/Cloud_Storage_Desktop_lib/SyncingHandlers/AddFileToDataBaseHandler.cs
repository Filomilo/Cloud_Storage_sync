using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Server.Handlers
{
    public class AddFileToDataBaseHandler : AbstactHandler
    {
        private ILogger _logger = CloudDriveLogging.Instance.GetLogger("AddFileToDataBaseHandler");
        private IConfiguration _configuration;
        private IFileRepositoryService _fileRepositoryService;

        public AddFileToDataBaseHandler(
            IConfiguration configuration,
            IFileRepositoryService fileRepositoryService
        )
        {
            _configuration = configuration;
            _fileRepositoryService = fileRepositoryService;
        }

        public override object Handle(object request)
        {
            List<UploudFileData> data = null;
            if (request is UploudFileData)
            {
                data = new List<UploudFileData>() { request as UploudFileData };
            }

            if (request is List<UploudFileData>)
            {
                data = request as List<UploudFileData>;
            }
            if (data == null)
            {
                throw new ArgumentException(
                    "AddFileToDataBaseHandler excepts argument of type UploudFileData or List Of Uplooud FIle data"
                );
            }

            List<LocalFileData> locals = new List<LocalFileData>();
            foreach (UploudFileData fileUpload in data)
            {
                LocalFileData local = new LocalFileData(fileUpload);
                if (
                    this._fileRepositoryService.DoesFileExist(
                        fileUpload,
                        out LocalFileData exisitngFile
                    )
                )
                {
                    _logger.LogDebug(
                        $"File {fileUpload.Name} already exists in the database. Updating version. from {local.Version}"
                    );
                    local.Version = exisitngFile.Version + 1;
                    this._fileRepositoryService.UpdateFile(exisitngFile, local);
                }
                else
                {
                    this._fileRepositoryService.AddNewFile(local);
                }
                locals.Add(local);
            }

            object returnVal = (request is List<UploudFileData>) ? locals : locals[0];
            if (_nextHandler != null)
            {
                _nextHandler.Handle(returnVal);
            }

            return returnVal;
        }
    }
}
