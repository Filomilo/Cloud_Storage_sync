using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;
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
            UpdateFileDataMessage update = null;
            if (request is UpdateFileDataMessage)
            {
                UpdateFileDataMessage fileUpdateMessage = (UpdateFileDataMessage)request;
                update = fileUpdateMessage;
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
                {
                    return this._nextHandler.Handle(new UpdateFileDataMessageRequest()
                    {
                        ExcludedDevices = new List<string>(){update.DeviceReuqested},
                        UserIdToSendTo = update.UserID,
                        updateFileDataMessage = update
                    });
                }
                    
                return null;
            }

            SyncFileData newFileVersion;
            using (var ctx = _dataBaseContextGenerator.GetDbContext())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        SyncFileData dbFileData = GetOldFileEnntryIDataBase(ctx, update);
                        newFileVersion = GetNewFileDataEntryInDataBase(ctx, update);
                        if (dbFileData != null)
                        {
                            RemoveOwnerFromDatabaseEntry(dbFileData, update, ctx);
                        }

                        if (newFileVersion == null)
                        {
                            newFileVersion = CreateNewDataEntryNewFileVersion(
                                dbFileData,
                                update,
                                ctx
                            );
                        }
                        else
                        {
                            AddDeviceToNewDBEntry(ctx, newFileVersion, update);
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


            UpdateFileDataMessageRequest updateFileDataMessageRequest = new UpdateFileDataMessageRequest()
            {
                updateFileDataMessage = update,
                UserIdToSendTo = update.UserID,
                InlcudedDevices = update.oldFileData.DeviceOwner,
                ExcludedDevices = new List<string>() { update.DeviceReuqested }
            };


            if (_nextHandler != null)
                return this._nextHandler.Handle(updateFileDataMessageRequest);
            return update;
        }

        private static SyncFileData? GetNewFileDataEntryInDataBase(
            AbstractDataBaseContext ctx,
            UpdateFileDataMessage update
        )
        {
            SyncFileData syncFile = null;
            try
            {
                Awaiters.AwaitTrue(() =>
                {
                    syncFile = ctx
                        .Files.ToList().OrderByDescending(x=>x.Version)
                        .FirstOrDefault(x =>
                            x.GetRealativePath().Equals(update.newFileData.GetRealativePath())
                            && !x.DeviceOwner.Contains(update.DeviceReuqested)
                            && x.Version > update.oldFileData.Version
                        );
                    return syncFile != null;
                });
            }
            catch (Exception)
            {
                int a = 0;
            }

            return syncFile;
        }

        private static SyncFileData? GetOldFileEnntryIDataBase(
            AbstractDataBaseContext ctx,
            UpdateFileDataMessage update
        )
        {
            return ctx
                .Files.ToList()
                .FirstOrDefault(x =>
                    x.GetRealativePath().Equals(update.oldFileData.GetRealativePath())
                    && x.DeviceOwner.Contains(update.DeviceReuqested)
                    && x.Version == update.oldFileData.Version
                );
        }

        private static void AddDeviceToNewDBEntry(
            AbstractDataBaseContext ctx,
            SyncFileData newFileVersion,
            UpdateFileDataMessage update
        )
        {
            //var trackedEntity = ctx
            //    .ChangeTracker.Entries<SyncFileData>()
            //    .FirstOrDefault(e =>
            //        e.Entity.Id == newFileVersion.Id
            //        && e.Entity.Path == newFileVersion.Path
            //        && e.Entity.Name == newFileVersion.Name
            //        && e.Entity.Extenstion == newFileVersion.Extenstion
            //    );

            //if (trackedEntity != null)
            //{
            //    trackedEntity.State = EntityState.Detached;
            //}
            SyncFileData updateDataNewFile = newFileVersion.Clone();
            updateDataNewFile.DeviceOwner.Add(update.DeviceReuqested);
            FileRepository.UpdateFile(ctx, newFileVersion, updateDataNewFile);
            ctx.SaveChangesAsync().Wait();
        }

        private static SyncFileData CreateNewDataEntryNewFileVersion(
            SyncFileData? dbFileData,
            UpdateFileDataMessage update,
            AbstractDataBaseContext ctx
        )
        {
            SyncFileData newFileVersion;
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
            return newFileVersion;
        }

        private static void RemoveOwnerFromDatabaseEntry(
            SyncFileData dbFileData,
            UpdateFileDataMessage update,
            AbstractDataBaseContext ctx
        )
        {
            SyncFileData updateData = dbFileData.Clone();
            updateData.DeviceOwner.Remove(update.DeviceReuqested);

            FileRepository.UpdateFile(ctx, dbFileData, updateData);

            ctx.SaveChangesAsync().Wait();
            ctx.Entry(dbFileData).State = EntityState.Detached;
        }
    }
}
