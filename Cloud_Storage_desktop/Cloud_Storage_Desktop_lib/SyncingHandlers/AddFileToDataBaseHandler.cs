using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Server.Handlers
{
    public class AddFileToDataBaseHandler : AbstactHandler
    {
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
            if (request is not UploudFileData)
            {
                throw new ArgumentException(
                    "AddFileToDataBaseHandler excepts argument of type UploudFileData"
                );
            }

            UploudFileData fileUpload = request as UploudFileData;
            LocalFileData local = new LocalFileData(fileUpload);
            if (
                this._fileRepositoryService.DoesFileExist(
                    fileUpload,
                    out LocalFileData exisitngFile
                )
            )
            {
                local.Version = exisitngFile.Version + 1;
                this._fileRepositoryService.UpdateFile(exisitngFile, local);
            }
            else
            {
                this._fileRepositoryService.AddNewFile(local);
            }

            if (_nextHandler != null)
            {
                _nextHandler.Handle(local);
            }

            return local;
        }
    }
}
