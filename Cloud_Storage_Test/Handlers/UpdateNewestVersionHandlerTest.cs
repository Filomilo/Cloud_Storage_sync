using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Handlers;
using Cloud_Storage_Server.Interfaces;
using NUnit.Framework;

namespace Cloud_Storage_Test.Handlers
{
    [TestFixture]
    class UpdateNewestVersionHandlerTest
    {
        private IDataBaseContextGenerator _dataBaseContextGenerator =
            new TestDataBaseSerwerContextGenerator();
        UpdateNewestVersionHandler updateNewestVersionHandler;
        TestHadnlerChecker testHadnlerChecker = new TestHadnlerChecker();
        List<SyncFileData> startingDb = new List<SyncFileData>()
        {
            new SyncFileData()
            {
                Id = Guid.NewGuid(),
                Name = "test",
                Extenstion = "txt",
                Path = "/test/test.txt",
                Hash = "111111111111",
                Version = 0,
                OwnerId = 1,
                DeviceOwner = new List<string>() { "test" },
            },
            new SyncFileData()
            {
                Id = Guid.NewGuid(),
                Name = "test",
                Extenstion = "txt",
                Path = "/test/test.txt",
                Hash = "22222222222",
                Version = 1,
                OwnerId = 1,
                DeviceOwner = new List<string>() { "test" },
            },
            new SyncFileData()
            {
                Id = Guid.NewGuid(),
                Name = "test",
                Extenstion = "txt",
                Path = "/test/test.txt",
                Hash = "333333333333333",
                Version = 2,
                OwnerId = 1,
                DeviceOwner = new List<string>() { "test" },
            },
        };

        [SetUp]
        public void setup()
        {
            _dataBaseContextGenerator.GetDbContext().Database.EnsureDeleted();
            _dataBaseContextGenerator.GetDbContext().Database.EnsureCreated();
            updateNewestVersionHandler = new UpdateNewestVersionHandler(_dataBaseContextGenerator);

            updateNewestVersionHandler.SetNext(testHadnlerChecker);
            using (var ctx = _dataBaseContextGenerator.GetDbContext())
            {
                startingDb.ForEach(x =>
                {
                    ctx.Files.Add(x);
                });

                ctx.SaveChanges();
            }
        }

        [Test]
        public void bringBackOldestVesrion()
        {
            using (var ctx = _dataBaseContextGenerator.GetDbContext())
            {
                Assert.That(FileRepository.GetAllUserFiles(ctx, 1).Count == startingDb.Count);
            }

            updateNewestVersionHandler.Handle(
                new UpdateNewestVersionRequest()
                {
                    userID = 1,
                    fileId = startingDb[0].Id.ToString(),
                    fileVession = 0,
                }
            );

            using (var ctx = _dataBaseContextGenerator.GetDbContext())
            {
                Assert.That(
                    FileRepository.GetAllUserFiles(ctx, 1).Count == startingDb.Count,
                    $"Expected ifles in databse to be the aame as satrting"
                );
            }
            using (var ctx = _dataBaseContextGenerator.GetDbContext())
            {
                SyncFileData newest = FileRepository
                    .GetAllUserFiles(ctx, 1)
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefault();
                Assert.That(newest != null);
                Assert.That(newest.Hash.Equals(startingDb[0].Hash));
            }

            Assert.That(
                testHadnlerChecker.didRun == true,
                "Expected to have a request handled by the next handler"
            );
        }
    }
}
