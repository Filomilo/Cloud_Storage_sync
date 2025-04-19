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

namespace Cloud_Storage_Test.Handlers
{
    [TestFixture]
    class UpdateIfOnlyOwnerChangedTest
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator =
            new TestDataBaseSerwerContextGenerator();
        private UpdateIfOnlyOwnerChanged updateIfOnlyOwnerChanged;
        private TestHadnlerChecker testHadnlerChecker;

        public User testUser;
        public Device testDevice1;
        public Device testDevice2;
        public SyncFileData testFileOld;
        public SyncFileData testFileNew;

        [SetUp]
        public void setup()
        {
            _dataBaseContextGenerator.GetDbContext().Database.EnsureDeleted();
            _dataBaseContextGenerator.GetDbContext().Database.EnsureCreated();
            testHadnlerChecker = new TestHadnlerChecker();

            updateIfOnlyOwnerChanged = new UpdateIfOnlyOwnerChanged(_dataBaseContextGenerator);
            updateIfOnlyOwnerChanged.SetNext(testHadnlerChecker);
            using (var ctx = _dataBaseContextGenerator.GetDbContext())
            {
                testUser = ctx
                    .Users.Add(new User() { mail = "mail@mail.con", password = "pass" })
                    .Entity;
                testDevice1 = ctx.Devices.Add(new Device() { OwnerId = testUser.id }).Entity;
                testDevice2 = ctx.Devices.Add(new Device() { OwnerId = testUser.id }).Entity;
                testFileNew = ctx
                    .Files.Add(
                        new SyncFileData()
                        {
                            Name = "test",
                            Extenstion = "txt",
                            Path = "/test/test.txt",
                            Hash = "1234567890",
                            OwnerId = testUser.id,
                            DeviceOwner = new List<string>() { testDevice1.Id.ToString() },
                            Version = 1,
                        }
                    )
                    .Entity;
                testFileOld = ctx
                    .Files.Add(
                        new SyncFileData()
                        {
                            Name = "test",
                            Extenstion = "txt",
                            Path = "/test/test.txt",
                            Hash = "1234567890",
                            OwnerId = testUser.id,
                            DeviceOwner = new List<string>() { testDevice2.Id.ToString() },
                            Version = 0,
                        }
                    )
                    .Entity;
                ctx.SaveChanges();
            }
        }

        public static IEnumerable<
            Func<UpdateIfOnlyOwnerChangedTest, Cloud_Storage_Server.Services.FileUploadRequest>
        > ShouldSkip()
        {
            yield return (updateIfOnlyOwnerChangedTest) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = updateIfOnlyOwnerChangedTest.testFileNew.Name,
                        Extenstion = updateIfOnlyOwnerChangedTest.testFileNew.Extenstion,
                        Path = updateIfOnlyOwnerChangedTest.testFileNew.Path,
                        Hash = updateIfOnlyOwnerChangedTest.testFileNew.Hash,
                        OwnerId = updateIfOnlyOwnerChangedTest.testFileNew.OwnerId,
                        DeviceOwner = updateIfOnlyOwnerChangedTest.testFileNew.DeviceOwner,
                    },
                    null
                );
            };

            yield return (updateIfOnlyOwnerChangedTest) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = updateIfOnlyOwnerChangedTest.testFileNew.Name + "---",
                        Extenstion = updateIfOnlyOwnerChangedTest.testFileNew.Extenstion,
                        Path = updateIfOnlyOwnerChangedTest.testFileNew.Path,
                        Hash = updateIfOnlyOwnerChangedTest.testFileNew.Hash,
                        OwnerId = updateIfOnlyOwnerChangedTest.testFileNew.OwnerId,
                        DeviceOwner = updateIfOnlyOwnerChangedTest.testFileNew.DeviceOwner,
                    },
                    null
                );
            };

            yield return (updateIfOnlyOwnerChangedTest) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = updateIfOnlyOwnerChangedTest.testFileNew.Name,
                        Extenstion = updateIfOnlyOwnerChangedTest.testFileNew.Extenstion + "a",
                        Path = updateIfOnlyOwnerChangedTest.testFileNew.Path,
                        Hash = updateIfOnlyOwnerChangedTest.testFileNew.Hash,
                        OwnerId = updateIfOnlyOwnerChangedTest.testFileNew.OwnerId,
                        DeviceOwner = updateIfOnlyOwnerChangedTest.testFileNew.DeviceOwner,
                    },
                    null
                );
            };

            yield return (updateIfOnlyOwnerChangedTest) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = updateIfOnlyOwnerChangedTest.testFileNew.Name,
                        Extenstion = updateIfOnlyOwnerChangedTest.testFileNew.Extenstion,
                        Path = ".",
                        Hash = updateIfOnlyOwnerChangedTest.testFileNew.Hash,
                        OwnerId = updateIfOnlyOwnerChangedTest.testFileNew.OwnerId,
                        DeviceOwner = updateIfOnlyOwnerChangedTest.testFileNew.DeviceOwner,
                    },
                    null
                );
            };
            yield return (updateIfOnlyOwnerChangedTest) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = updateIfOnlyOwnerChangedTest.testFileNew.Name,
                        Extenstion = updateIfOnlyOwnerChangedTest.testFileNew.Extenstion,
                        Path = updateIfOnlyOwnerChangedTest.testFileNew.Path,
                        Hash = "hash",
                        OwnerId = updateIfOnlyOwnerChangedTest.testFileNew.OwnerId,
                        DeviceOwner = updateIfOnlyOwnerChangedTest.testFileNew.DeviceOwner,
                    },
                    null
                );
            };
            yield return (updateIfOnlyOwnerChangedTest) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = updateIfOnlyOwnerChangedTest.testFileNew.Name,
                        Extenstion = updateIfOnlyOwnerChangedTest.testFileNew.Extenstion + "a",
                        Path = updateIfOnlyOwnerChangedTest.testFileNew.Path,
                        Hash = updateIfOnlyOwnerChangedTest.testFileNew.Hash,
                        OwnerId = 5,
                        DeviceOwner = updateIfOnlyOwnerChangedTest.testFileNew.DeviceOwner,
                    },
                    null
                );
            };
        }

        public static IEnumerable<
            Func<UpdateIfOnlyOwnerChangedTest, Cloud_Storage_Server.Services.FileUploadRequest>
        > ShouldUpdate()
        {
            yield return (updateIfOnlyOwnerChangedTest) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = updateIfOnlyOwnerChangedTest.testFileNew.Name,
                        Extenstion = updateIfOnlyOwnerChangedTest.testFileNew.Extenstion,
                        Path = updateIfOnlyOwnerChangedTest.testFileNew.Path,
                        Hash = updateIfOnlyOwnerChangedTest.testFileNew.Hash,
                        OwnerId = updateIfOnlyOwnerChangedTest.testFileNew.OwnerId,
                        DeviceOwner = new List<string>()
                        {
                            updateIfOnlyOwnerChangedTest.testDevice2.Id.ToString(),
                        },
                    },
                    null
                );
            };
        }

        //private FileUploudRequest[] ShoudlSkip;

        //private FileUploudRequest[] ShouldNotSkip;

        [Test]
        public void SkipIfTheSameFileExist_shoulUpdate(
            [ValueSource("ShouldUpdate")]
                Func<
                UpdateIfOnlyOwnerChangedTest,
                Cloud_Storage_Server.Services.FileUploadRequest
            > getReq
        )
        {
            Cloud_Storage_Server.Services.FileUploadRequest req = getReq.Invoke(this);
            this.updateIfOnlyOwnerChanged.Handle(req);
            using (var ctx = this._dataBaseContextGenerator.GetDbContext())
            {
                List<SyncFileData> files = ctx.Files.ToList();
                Assert.That(
                    files.Count == 2,
                    $"Error in db is {files.Count} files while it should be 1"
                );
                SyncFileData fileInDbNew = files[0];
                SyncFileData fileInDOld = files[1];
                Assert.That(
                    fileInDOld.DeviceOwner.Count == 0,
                    $"Old file should have zero owners but is :: \n {fileInDOld}"
                );

                Assert.That(
                    fileInDbNew.DeviceOwner.Contains(req.syncFileData.DeviceOwner.First()),
                    $"new file should have new owners but is :: \n {fileInDbNew}"
                );
            }

            Assert.That(this.testHadnlerChecker.didRun == false, "Error next hadnler did run");
        }

        [Test]
        public void SkipIfTheSameFileExist_shouldDoNothing(
            [ValueSource("ShouldSkip")]
                Func<
                UpdateIfOnlyOwnerChangedTest,
                Cloud_Storage_Server.Services.FileUploadRequest
            > getReq
        )
        {
            Cloud_Storage_Server.Services.FileUploadRequest req = getReq.Invoke(this);
            this.updateIfOnlyOwnerChanged.Handle(req);
            using (var ctx = this._dataBaseContextGenerator.GetDbContext())
            {
                List<SyncFileData> files = ctx
                    .Files.ToList()
                    .OrderByDescending(x => x.Version)
                    .ToList();
                Assert.That(
                    files.Count == 2,
                    $"Error in db is {files.Count} files while it should be 2"
                );
                SyncFileData fileInDbNew = files[0];
                SyncFileData fileInDOld = files[1];
                Assert.That(
                    fileInDbNew.Equals(this.testFileNew),
                    $"Error file should be the same orignal file data: \n {testFileNew}\n new file data: \n{fileInDbNew}"
                );
                Assert.That(
                    fileInDOld.Equals(this.testFileOld),
                    $"Error file should be the same orignal file data: \n {testFileOld}\n new file data: \n{fileInDOld}"
                );
            }

            Assert.That(this.testHadnlerChecker.didRun == true, "Error next hadnler did not run ");
        }
    }
}
