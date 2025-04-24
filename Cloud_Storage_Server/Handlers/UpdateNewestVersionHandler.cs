using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;

namespace Cloud_Storage_Server.Handlers
{
    public class UpdateNewestVersionRequest
    {
        public long userID { get; set; }
        public string fileId { get; set; }
        public ulong fileVession { get; set; }
    }

    public class UpdateNewestVersionHandler : AbstactHandler
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator;

        public UpdateNewestVersionHandler(IDataBaseContextGenerator dataBaseContextGenerator)
        {
            this._dataBaseContextGenerator = dataBaseContextGenerator;
        }

        public override object Handle(object request)
        {
            UpdateNewestVersionRequest req = null;
            if (request is UpdateNewestVersionRequest)
            {
                req = request as UpdateNewestVersionRequest;
            }

            if (req == null)
            {
                throw new ArgumentException(
                    "UpdateNewestVersionHandler excepts argument of type UpdateNewestVersionRequest"
                );
            }

            SyncFileData fileInDataBase = null;
            using (var ctx = this._dataBaseContextGenerator.GetDbContext())
            {
                fileInDataBase = getFileRepressentionInDb(ctx, req);
                SyncFileData newestFileInRepositoryNow =
                    FileRepository.getNewestFileByPathNameExtensionAndUser(
                        ctx,
                        fileInDataBase.Path,
                        fileInDataBase.Name,
                        fileInDataBase.Extenstion,
                        req.userID
                    );
                if (
                    newestFileInRepositoryNow != null
                    && newestFileInRepositoryNow.Version != fileInDataBase.Version
                )
                {
                    //newestFileData = fileInDataBase.Clone();
                    fileInDataBase.Version = newestFileInRepositoryNow.Version + 1;
                    ctx.Files.Update(fileInDataBase);
                    ctx.SaveChangesAsync().Wait();
                }
                else
                {
                    throw new ArgumentException("File not found");
                }
            }

            if (_nextHandler != null && fileInDataBase != null)
                return this._nextHandler.Handle(fileInDataBase);
            return fileInDataBase;
        }

        private static SyncFileData? getFileRepressentionInDb(
            AbstractDataBaseContext ctx,
            UpdateNewestVersionRequest req
        )
        {
            SyncFileData fileInDataBase = ctx
                .Files.Where(x =>
                    x.Id.Equals(Guid.Parse(req.fileId))
                    && x.OwnerId.Equals(req.userID)
                    && req.fileVession.Equals(req.fileVession)
                )
                .FirstOrDefault();
            if (fileInDataBase != null)
            {
                ctx.SaveChangesAsync().Wait();
            }
            else
            {
                throw new ArgumentException("File not found");
            }

            return fileInDataBase;
        }
    }
}
