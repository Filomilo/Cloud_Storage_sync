using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Handlers;
using Cloud_Storage_Server.Interfaces;
using NUnit.Framework;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Cloud_Storage_Test.Handlers
{
    [TestFixture]
    public class RenameIfOnlyPathChangedHandlerTest
    {
        private RenameIfOnlyPathChangedHandler handler;
        private IDataBaseContextGenerator dataBaseContextGenerator;
        private Cloud_Storage_Server.Database.Models.User user1;
        private Cloud_Storage_Common.Models.Device device1;
        private Cloud_Storage_Common.Models.Device device2;

        [SetUp]
        public void setup()
        {
            user1 = new Cloud_Storage_Server.Database.Models.User();
            user1.mail = "user1@user.com";
            user1.password = "Password+123";

            device1 = new Cloud_Storage_Common.Models.Device();
            device1.Owner = user1;

            device2 = new Cloud_Storage_Common.Models.Device();
            device2.Owner = user1;

            dataBaseContextGenerator = new TestDataBaseSerwerContextGenerator();
            using (var ctx = dataBaseContextGenerator.GetDbContext())
            {
                user1 = ctx.Users.Add(user1).Entity;
                device1 = ctx.Devices.Add(device1).Entity;
                device2 = ctx.Devices.Add(device2).Entity;
                ctx.SaveChanges();
            }

            dataBaseContextGenerator.GetDbContext().Database.EnsureCreated();
            handler = new RenameIfOnlyPathChangedHandler(dataBaseContextGenerator);
        }

        [TearDown]
        public void tearDown()
        {
            dataBaseContextGenerator.GetDbContext().Database.EnsureDeleted();
        }

        [Test]
        public void RenameIfOnlyPathChangedForOwnerUser()
        {
            {
                SyncFileData syncFileDataExisitng = TestHelpers.CreateSyncFileData();
                syncFileDataExisitng.DeviceOwner.Add(device1.Id.ToString());
                using (var ctx = this.dataBaseContextGenerator.GetDbContext())
                {
                    ctx.Files.Add(syncFileDataExisitng);
                    ctx.SaveChanges();
                }

                SyncFileData newSyncFileData = syncFileDataExisitng.Clone();
                newSyncFileData.Name = "newName";
                newSyncFileData.Version++;
                UpdateFileDataMessage message = new UpdateFileDataMessage()
                {
                    DeviceReuqested = device1.Id.ToString(),
                    newFileData = newSyncFileData,
                    oldFileData = syncFileDataExisitng,
                    UpdateType = UPDATE_TYPE.RENAME,
                    UserID = user1.id,
                };
                handler.Handle(message);

                using (var ctx = this.dataBaseContextGenerator.GetDbContext())
                {
                    Assert.That(ctx.Files.Count() == 2);
                    Assert.That(
                        ctx.Files.Where(x => x.DeviceOwner.Count == 0).Count() == 1,
                        $"There sohuld be on file withou owner but are :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );
                    Assert.That(
                        ctx.Files.Where(x => x.DeviceOwner.Count == 1).Count() == 1,
                        $"There sohuld be on file with owner but are :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );
                    Assert.That(
                        ctx.Files.Where(x => x.Version == 1).Count() == 1,
                        $"There sohuld be on file With version 1 :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );
                    Assert.That(
                        ctx.Files.Where(x => x.Version == 0).Count() == 1,
                        $"There sohuld be on file With version 0 :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );
                }
            }
        }

        [Test]
        public void RenameIfOnlyPathChangedForNewDevice()
        {
            {
                SyncFileData syncFileDataExisitng = TestHelpers.CreateSyncFileData();
                syncFileDataExisitng.DeviceOwner.Add(device1.Id.ToString());
                syncFileDataExisitng.DeviceOwner.Add(device2.Id.ToString());
                using (var ctx = this.dataBaseContextGenerator.GetDbContext())
                {
                    ctx.Files.Add(syncFileDataExisitng);
                    ctx.SaveChanges();
                }

                SyncFileData newSyncFileData = syncFileDataExisitng.Clone();
                newSyncFileData.Name = "newName";
                newSyncFileData.Version++;
                UpdateFileDataMessage message = new UpdateFileDataMessage()
                {
                    DeviceReuqested = device1.Id.ToString(),
                    newFileData = newSyncFileData,
                    oldFileData = syncFileDataExisitng,
                    UpdateType = UPDATE_TYPE.RENAME,
                    UserID = user1.id,
                };
                handler.Handle(message);

                using (var ctx = this.dataBaseContextGenerator.GetDbContext())
                {
                    Assert.That(ctx.Files.Count() == 2);

                    Assert.That(
                        ctx.Files.Where(x => x.DeviceOwner.Count == 1).Count() == 2,
                        $"There sohuld be 2 file with owner but are :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );

                    Assert.That(
                        ctx.Files.Where(x => x.DeviceOwner.Contains(device1.Id.ToString())).Count()
                            == 1,
                        $"There sohuld be 1 file with owner dveice 1 but are :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );
                    Assert.That(
                        ctx.Files.Where(x => x.DeviceOwner.Contains(device2.Id.ToString())).Count()
                            == 1,
                        $"There sohuld be 1 file with owner dveice 1 but are :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );
                }

                message.DeviceReuqested = device2.Id.ToString();
                handler.Handle(message);

                using (var ctx = this.dataBaseContextGenerator.GetDbContext())
                {
                    Assert.That(ctx.Files.Count() == 2);

                    Assert.That(
                        ctx.Files.Where(x => x.DeviceOwner.Count == 2).Count() == 1,
                        $"There sohuld be 1 file with owner but are :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );
                    Assert.That(
                        ctx.Files.Where(x => x.DeviceOwner.Count == 0).Count() == 1,
                        $"There sohuld be 1 file without owner but are :: \n [[{string.Join(", \n", ctx.Files)}]]"
                    );
                }
            }
        }
    }
}
