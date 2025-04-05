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
using Microsoft.AspNetCore.Razor.TagHelpers;
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

        [Test]
        public void Connect_With_Files_Already_OnDisk()
        {
            #region Ensure not connected and 3 files

            int amountOfFiles = 3;
            var getFileContent = (int num) =>
            {
                return $"Content_{num}";
            };

            List<string> filesAdded = new List<string>();
            for (int i = 0; i < amountOfFiles; i++)
            {
                filesAdded.Add(
                    TestHelpers.CreateTmpFile(
                        this._Client1Config.StorageLocation,
                        getFileContent(i)
                    )
                );
            }

            Assert.That(
                FileRepository.GetAllUserFiles(UserRepository.getUserByMail(email).id).Count == 0,
                "File for user in reposirotry not 0"
            );

            #endregion


            #region Connect to server
            this._cloudDriveSyncSystemClient1.ServerConnection.login(email, pass);

            Assert.That(
                this._cloudDriveSyncSystemClient1.ServerConnection.CheckIfAuthirized,
                "cloud drive system 1 is no authorized"
            );

            #endregion

            #region Ensure the same data on server and clinet

            Assert.DoesNotThrow(
                () =>
                    TestHelpers.EnsureTrue(() =>
                    {
                        return FileRepository
                                .GetAllUserFiles(UserRepository.getUserByMail(email).id)
                                .Count == amountOfFiles;
                    })
            );

            CheckIfTheSameContentOnClinetsAndServer(
                new List<CloudDriveSyncSystem>() { this._cloudDriveSyncSystemClient1 }
            );

            #endregion
        }

        [Test]
        public void Connect_With_Files_On_Server()
        {
            throw new NotImplementedException("test not implemented");

            #region Ensure nor connected and 3 files on server


            #endregion


            #region Connect to server


            #endregion

            #region Esnure correct file ownership


            #endregion

            #region Ensure the same data on server and clinet


            #endregion
        }

        [Test]
        public void Connect_With_Diffrent_Files_On_Server_And_Device()
        {
            throw new NotImplementedException("test not implemented");

            #region Ensure diffrent files on server and device


            #endregion


            #region Connect to server
            this._cloudDriveSyncSystemClient1.ServerConnection.login(email, pass);

            #endregion

            #region Esnure correct file ownership


            #endregion

            #region Ensure the same data on server and clinet


            #endregion
        }

        [Test]
        public void Delete_File_Located_On_Both_Devices()
        {
            throw new NotImplementedException("test not implemented");

            #region Ensure The same file


            #endregion


            #region Connect to server


            #endregion

            #region Esnure correct file ownership


            #endregion

            #region Ensure the same data on server and clinet


            #endregion
        }

        #region HelpersMethod

        private string AddFileToBothServerAndClients()
        {
            throw new NotImplementedException("AddFileToBothServerAndClients not implemented");
        }

        private void CheckIfTheSameContentOnClinetsAndServer(List<CloudDriveSyncSystem> systems)
        {
            List<SyncFileData> filesOnTheServer = FileRepository.GetAllUserFiles(
                UserRepository.getUserByMail(email).id
            );
            foreach (CloudDriveSyncSystem cloudDriveSyncSystem in systems)
            {
                List<FileData> filesInUserLocation = FileManager.GetAllFilesInLocationRelative(
                    cloudDriveSyncSystem.Configuration.StorageLocation
                );
                Assert.That(
                    filesInUserLocation.Count == filesOnTheServer.Count,
                    $"Files on device {cloudDriveSyncSystem.Configuration.DeviceUUID}"
                );
                foreach (SyncFileData syncFileData in filesOnTheServer)
                {
                    string serverFileHash = FileManager.GetHashOfFile(
                        this._fileSystemService.GetFullPathToFile(syncFileData)
                    );

                    FileData correspoidingFile = filesInUserLocation.Find(x =>
                        x.Name == syncFileData.Name
                        && x.Path == syncFileData.Path
                        && x.Extenstion == syncFileData.Extenstion
                    );

                    string loacalHash = FileManager.GetHashOfFile(
                        correspoidingFile.getFullFilePathForBasePath(
                            cloudDriveSyncSystem.Configuration.StorageLocation
                        )
                    );
                    Assert.That(
                        loacalHash == serverFileHash,
                        "Content of file {correspoidingFile} diffrent from server and device"
                    );
                }
            }
        }

        #endregion
    }
}
