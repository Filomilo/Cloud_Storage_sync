using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Controllers;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Handlers;
using Cloud_Storage_Server.Interfaces;
using NUnit.Framework;

namespace Cloud_Storage_Test.Handlers
{
    [TestFixture]
    class SkipIfTheSameFileExist
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator =
            new TestDataBaseSerwerContextGenerator();
        private SkipIfTheSameFileAlreadyExist skipIfTheSameFileAlreadyExist;
        private TestHadnlerChecker testHadnlerChecker;

        private User testUser;
        private Device testDevice1;
        private Device testDevice2;
        private SyncFileData testFile1;

        [SetUp]
        public void setup()
        {
            _dataBaseContextGenerator.GetDbContext().Database.EnsureDeleted();
            _dataBaseContextGenerator.GetDbContext().Database.EnsureCreated();
            testHadnlerChecker = new TestHadnlerChecker();

            skipIfTheSameFileAlreadyExist = new SkipIfTheSameFileAlreadyExist(
                _dataBaseContextGenerator
            );
            skipIfTheSameFileAlreadyExist.SetNext(testHadnlerChecker);
            using (var ctx = _dataBaseContextGenerator.GetDbContext())
            {
                testUser = ctx
                    .Users.Add(new User() { mail = "mail@mail.con", password = "pass" })
                    .Entity;
                testDevice1 = ctx.Devices.Add(new Device() { OwnerId = testUser.id }).Entity;
                testDevice2 = ctx.Devices.Add(new Device() { OwnerId = testUser.id }).Entity;
                testFile1 = ctx
                    .Files.Add(
                        new SyncFileData()
                        {
                            Name = "test",
                            Extenstion = "txt",
                            Path = "/test/test.txt",
                            Hash = "1234567890",
                            OwnerId = testUser.id,
                            DeviceOwner = new List<string>() { testDevice1.Id.ToString() },
                        }
                    )
                    .Entity;
                ctx.SaveChanges();
            }
        }

        public static IEnumerable<
            Func<SkipIfTheSameFileExist, Cloud_Storage_Server.Services.FileUploadRequest>
        > ShouldSkip()
        {
            yield return (skipIfTheSameFileExist) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = skipIfTheSameFileExist.testFile1.Name,
                        Extenstion = skipIfTheSameFileExist.testFile1.Extenstion,
                        Path = skipIfTheSameFileExist.testFile1.Path,
                        Hash = skipIfTheSameFileExist.testFile1.Hash,
                        OwnerId = skipIfTheSameFileExist.testFile1.OwnerId,
                        DeviceOwner = skipIfTheSameFileExist.testFile1.DeviceOwner,
                    },
                    null
                );
            };
        }

        public static IEnumerable<
            Func<SkipIfTheSameFileExist, Cloud_Storage_Server.Services.FileUploadRequest>
        > ShouldNotSkip()
        {
            yield return (skipIfTheSameFileExist) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = skipIfTheSameFileExist.testFile1.Name,
                        Extenstion = skipIfTheSameFileExist.testFile1.Extenstion,
                        Path = skipIfTheSameFileExist.testFile1.Path,
                        Hash = skipIfTheSameFileExist.testFile1.Hash,
                        OwnerId = skipIfTheSameFileExist.testFile1.OwnerId,
                        DeviceOwner = new List<string>()
                        {
                            skipIfTheSameFileExist.testDevice2.Id.ToString(),
                        },
                    },
                    null
                );
            };
            yield return (skipIfTheSameFileExist) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = skipIfTheSameFileExist.testFile1.Name,
                        Extenstion = skipIfTheSameFileExist.testFile1.Extenstion,
                        Path = skipIfTheSameFileExist.testFile1.Path,
                        Hash = skipIfTheSameFileExist.testFile1.Hash,
                        OwnerId = skipIfTheSameFileExist.testFile1.OwnerId + 1,
                        DeviceOwner = skipIfTheSameFileExist.testFile1.DeviceOwner,
                    },
                    null
                );
            };

            yield return (skipIfTheSameFileExist) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = skipIfTheSameFileExist.testFile1.Name,
                        Extenstion = skipIfTheSameFileExist.testFile1.Extenstion,
                        Path = skipIfTheSameFileExist.testFile1.Path,
                        Hash = "Diffrent hash",
                        OwnerId = skipIfTheSameFileExist.testFile1.OwnerId,
                        DeviceOwner = skipIfTheSameFileExist.testFile1.DeviceOwner,
                    },
                    null
                );
            };

            yield return (skipIfTheSameFileExist) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = skipIfTheSameFileExist.testFile1.Name,
                        Extenstion = skipIfTheSameFileExist.testFile1.Extenstion,
                        Path = ".",
                        Hash = skipIfTheSameFileExist.testFile1.Hash,
                        OwnerId = skipIfTheSameFileExist.testFile1.OwnerId,
                        DeviceOwner = skipIfTheSameFileExist.testFile1.DeviceOwner,
                    },
                    null
                );
            };

            yield return (skipIfTheSameFileExist) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = skipIfTheSameFileExist.testFile1.Name,
                        Extenstion = ".e",
                        Path = skipIfTheSameFileExist.testFile1.Path,
                        Hash = skipIfTheSameFileExist.testFile1.Hash,
                        OwnerId = skipIfTheSameFileExist.testFile1.OwnerId,
                        DeviceOwner = skipIfTheSameFileExist.testFile1.DeviceOwner,
                    },
                    null
                );
            };
            yield return (skipIfTheSameFileExist) =>
            {
                return new Cloud_Storage_Server.Services.FileUploadRequest(
                    new SyncFileData()
                    {
                        Name = "diffName",
                        Extenstion = skipIfTheSameFileExist.testFile1.Extenstion,
                        Path = skipIfTheSameFileExist.testFile1.Path,
                        Hash = skipIfTheSameFileExist.testFile1.Hash,
                        OwnerId = skipIfTheSameFileExist.testFile1.OwnerId,
                        DeviceOwner = skipIfTheSameFileExist.testFile1.DeviceOwner,
                    },
                    null
                );
            };
        }

        //private FileUploudRequest[] ShoudlSkip;

        //private FileUploudRequest[] ShouldNotSkip;

        [Test]
        public void SkipIfTheSameFileExist_shouldContinue(
            [ValueSource("ShouldNotSkip")]
                Func<SkipIfTheSameFileExist, Cloud_Storage_Server.Services.FileUploadRequest> getReq
        )
        {
            Cloud_Storage_Server.Services.FileUploadRequest req = getReq.Invoke(this);
            this.skipIfTheSameFileAlreadyExist.Handle(req);
            Assert.That(
                this.testHadnlerChecker.didRun == true,
                $"Error next hadnler did not run when it should for request:\n {req.syncFileData} \n while in db is :: \n {this.testFile1}"
            );
        }

        [Test]
        public void SkipIfTheSameFileExist_shouldStop(
            [ValueSource("ShouldSkip")]
                Func<SkipIfTheSameFileExist, Cloud_Storage_Server.Services.FileUploadRequest> getReq
        )
        {
            Cloud_Storage_Server.Services.FileUploadRequest req = getReq.Invoke(this);
            this.skipIfTheSameFileAlreadyExist.Handle(req);
            Assert.That(
                this.testHadnlerChecker.didRun == false,
                "Error next hadnler did run when it should not"
            );
        }
    }
}
