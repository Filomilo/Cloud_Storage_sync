using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Tests;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Cloud_Storage_Test
{
    [TestFixture]
    class MainTestCases
    {
        private CloudDriveSyncSystem _cloudDriveSyncSystemClient1;
        private CloudDriveSyncSystem _cloudDriveSyncSystemClient2;
        private IConfiguration _Client1Config;
        private IConfiguration _Client2Config;
        private HttpClient _testServer;
        private FileSystemService _fileSystemService = TestHelpers.GetDeafultFileSystemService();

        private string email;
        string pass = "1234567890asdASD++";

        [SetUp]
        public void Setup()
        {
            ILogger logger = CloudDriveLogging.Instance.GetLogger("TestLogging");
            logger.LogInformation("Test log-------------------------------------");
            CloudDriveLogging.Instance.SetTestLogger(logger);
            _testServer = new MyWebApplication().CreateDefaultClient();

            email = $"{Guid.NewGuid().ToString()}@mail.mail";

            string tmpDirectory1 = TestHelpers.GetNewTmpDir("user1");
            string tmpDirectory2 = TestHelpers.GetNewTmpDir("user2");
            Directory.CreateDirectory(tmpDirectory1);
            Directory.CreateDirectory(tmpDirectory2);

            _Client1Config = new TestConfig(tmpDirectory1, "Device_1");
            _Client2Config = new TestConfig(tmpDirectory2, "Device_2");

            this._cloudDriveSyncSystemClient1 = new CloudDriveSyncSystem(
                _testServer,
                _Client1Config,
                new TestCredentialMangager()
            );
            this._cloudDriveSyncSystemClient2 = new CloudDriveSyncSystem(
                _testServer,
                _Client2Config,
                new TestCredentialMangager()
            );
            _cloudDriveSyncSystemClient1.ServerConnection.Register(email, pass);
            _cloudDriveSyncSystemClient2.ServerConnection.login(email, pass);

            Assert.That(
                _cloudDriveSyncSystemClient1.ServerConnection.CheckIfAuthirized(),
                $"Cloud Drive sync system nr 1 is not authorized"
            );
            Assert.That(
                _cloudDriveSyncSystemClient2.ServerConnection.CheckIfAuthirized(),
                $"Cloud Drive sync system nr 2 is not authorized"
            );
            _cloudDriveSyncSystemClient1.ServerConnection.Logout();
            _cloudDriveSyncSystemClient2.ServerConnection.Logout();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.RemoveTmpDirectory();
        }

        [Test]
        public void Create_And_Sync_File_In_EmptyDirectory()
        {
            #region Ensure connected and empty
            this._cloudDriveSyncSystemClient1.ServerConnection.login(email, pass);
            Assert.That(
                _cloudDriveSyncSystemClient1.ServerConnection.CheckIfAuthirized(),
                $"Client is not authorized"
            );

            List<FileData> filesInSyncLocation = FileManager.GetAllFilesInLocation(
                _Client1Config.StorageLocation
            );
            Assert.That(
                filesInSyncLocation.Count == 0,
                "Stating sync location is not empty at beginning"
            );

            #endregion


            #region Create New File
            string fileContent = "Exmaple File Content_" + Guid.NewGuid();
            String createdFileName = TestHelpers.CreateTmpFile(
                _Client1Config.StorageLocation,
                fileContent
            );

            #endregion

            #region Ensure the same data on server and clinet

            List<SyncFileData> files = new List<SyncFileData>();
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        files = FileRepository.GetAllUserFiles(
                            UserRepository.getUserByMail(email).id
                        );
                        return files.Count == 1;
                    });
                },
                "File reposirotry did not reach files amount to zero in desired time"
            );

            SyncFileData syncesFile = files.First();

            Assert.That(
                syncesFile.Name == Path.GetFileNameWithoutExtension(createdFileName),
                $"File name in database {syncesFile.Name} is not equal to file name in folder {Path.GetFileNameWithoutExtension(createdFileName)}"
            );
            string originalFileHash = FileManager.GetHashOfFile(
                _Client1Config.StorageLocation + createdFileName
            );
            string serverFileHash = FileManager.GetHashOfFile(
                _fileSystemService.GetFullPathToFile(syncesFile)
            );

            Assert.That(
                serverFileHash.Equals(originalFileHash),
                $"orignal file hash \n[[{originalFileHash}]]\n IS not the same as file hash on server \n[[{serverFileHash}]]\n"
            );

            #endregion
        }
    }
}
