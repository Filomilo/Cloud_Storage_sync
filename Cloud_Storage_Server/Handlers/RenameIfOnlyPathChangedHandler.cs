using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Server.Handlers
{
    public class RenameIfOnlyPathChangedHandler : AbstactHandler
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator;

        public RenameIfOnlyPathChangedHandler(IDataBaseContextGenerator dataBaseContextGenerator)
        {
            _dataBaseContextGenerator = dataBaseContextGenerator;
        }

        public override object Handle(object request)
        {
            UpdateFileDataRequest update = null;
            if (request is UpdateFileDataRequest)
            {
                UpdateFileDataRequest fileUpdateRequest = (UpdateFileDataRequest)request;
                update = fileUpdateRequest;
            }

            if (update is null)
            {
                throw new ArgumentException(
                    "RenameIfOnlyPathChangedHandler excepts argument of type SyncFileData or UpdateFileDataRequest"
                );
            }

            if (update.oldFileData == null)
            {
                if (_nextHandler != null)
                    return this._nextHandler.Handle(update);
                return null;
            }

            SyncFileData newFileVersion;
            using (var ctx = _dataBaseContextGenerator.GetDbContext())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        SyncFileData dbFileData = ctx
                            .Files.ToList()
                            .FirstOrDefault(x =>
                                x.GetRealativePath().Equals(update.oldFileData.GetRealativePath())
                                && x.DeviceOwner.Contains(update.DeviceReuqested)
                                && x.Version == update.oldFileData.Version
                            );
                        newFileVersion = ctx
                            .Files.ToList()
                            .FirstOrDefault(x =>
                                x.GetRealativePath().Equals(update.newFileData.GetRealativePath())
                                && !x.DeviceOwner.Contains(update.DeviceReuqested)
                                && x.Version > update.oldFileData.Version
                            );
                        if (dbFileData != null)
                        {
                            SyncFileData updateData = dbFileData.Clone();
                            updateData.DeviceOwner.Remove(update.DeviceReuqested);
                            FileRepository.UpdateFile(ctx, dbFileData, updateData);

                            ctx.SaveChangesAsync().Wait();
                            ctx.Entry(dbFileData).State = EntityState.Detached;
                        }

                        if (newFileVersion == null)
                        {
                            newFileVersion = new SyncFileData()
                            {
                                Id = dbFileData == null ? Guid.NewGuid() : dbFileData.Id,
                                Path = update.newFileData.Path,
                                Name = update.newFileData.Name,
                                Extenstion = update.newFileData.Extenstion,
                                Hash = update.newFileData.Hash,
                                Version = update.newFileData.Version,
                                OwnerId = update.UserID,
                                DeviceOwner = new List<string>() { update.DeviceReuqested },
                                //SyncDate = DateTime.Now,
                                BytesSize = update.newFileData.BytesSize,
                            };

                            ctx.Files.Add(newFileVersion);

                            ctx.SaveChangesAsync().Wait();
                            ctx.Entry(newFileVersion).State = EntityState.Detached;
                        }
                        else
                        {
                            SyncFileData updateDataNewFile = newFileVersion.Clone();
                            updateDataNewFile.DeviceOwner.Add(update.DeviceReuqested);
                            FileRepository.UpdateFile(ctx, newFileVersion, updateDataNewFile);
                            ctx.SaveChangesAsync().Wait();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }

            update.newFileData = newFileVersion;

            if (_nextHandler != null)
                return this._nextHandler.Handle(update);
            return update;
        }
    }
}
