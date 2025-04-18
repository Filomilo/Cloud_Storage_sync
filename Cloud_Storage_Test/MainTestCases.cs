using System.Net.WebSockets;
using System.Text;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Tests;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
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
        private HttpClient _testServer1;
        private HttpClient _testServer2;
        private FileSystemService _fileSystemService = TestHelpers.GetDeafultFileSystemService();
        private IFileRepositoryService _localFileRepositoryService1;
        private MyWebApplication webApplication;

        private IFileRepositoryService _localFileRepositoryService2;
        private IWebsocketConnectedController websocketConnectedController;
        private string email;
        string pass = "1234567890asdASD++";

        [SetUp]
        public void Setup()
        {
            Thread.Sleep(2000);
            TestHelpers.ResetDatabase();
            TestHelpers.EnsureTrue(() =>
            {
                try
                {
                    using (var ctx = new DatabaseContext())
                    {
                        ctx.Users.ToList();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            });
            TestHelpers.RemoveTmpDirectory();
            _localFileRepositoryService1 =
                new Cloud_Storage_Desktop_lib.Services.FileRepositoryService(
                    new TestDbContextGenerator1()
                );
            _localFileRepositoryService2 =
                new Cloud_Storage_Desktop_lib.Services.FileRepositoryService(
                    new TestDbContextGenerator2()
                );

            ILogger logger = CloudDriveLogging.Instance.GetLogger("TestLogging");
            logger.LogInformation("Test log-------------------------------------");
            CloudDriveLogging.Instance.SetTestLogger(logger);
            webApplication = new MyWebApplication();

            _testServer1 = webApplication.CreateDefaultClient();
            _testServer2 = webApplication.CreateDefaultClient();

            websocketConnectedController = (IWebsocketConnectedController)
                webApplication.Server.Services.GetService(typeof(IWebsocketConnectedController));

            WebSocketClient webSocketClient = webApplication.Server.CreateWebSocketClient();
            email = $"{Guid.NewGuid().ToString()}@mail.mail";

            string tmpDirectory1 = TestHelpers.GetNewTmpDir("user1");
            string tmpDirectory2 = TestHelpers.GetNewTmpDir("user2");
            Directory.CreateDirectory(tmpDirectory1);
            Directory.CreateDirectory(tmpDirectory2);

            Random random = new Random();
            _Client1Config = new TestConfig(tmpDirectory1);
            _Client2Config = new TestConfig(tmpDirectory2);

            this._cloudDriveSyncSystemClient1 = new CloudDriveSyncSystem(
                _testServer1,
                new TestWebScoket(webSocketClient),
                _Client1Config,
                new TestCredentialMangager(),
                _localFileRepositoryService1
            );
            this._cloudDriveSyncSystemClient2 = new CloudDriveSyncSystem(
                _testServer2,
                new TestWebScoket(webSocketClient),
                _Client2Config,
                new TestCredentialMangager(),
                _localFileRepositoryService2
            );
            _cloudDriveSyncSystemClient1.ServerConnection.Register(email, pass);
            _cloudDriveSyncSystemClient2.ServerConnection.login(email, pass);
            Assert.DoesNotThrow(
                () =>
                {
                    Assert.That(
                        _cloudDriveSyncSystemClient1.CredentialManager.GetDeviceID() != null,
                        "_cloudDriveSyncSystemClient1 device id is null"
                    );
                    Assert.That(
                        _cloudDriveSyncSystemClient2.CredentialManager.GetDeviceID() != null,
                        "_cloudDriveSyncSystemClient2 device id is null"
                    );
                },
                "couldn't proprly retive device id from token"
            );

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
            this._cloudDriveSyncSystemClient1.SystemWatcher.Stop();
            this._cloudDriveSyncSystemClient2.SystemWatcher.Stop();
            TestHelpers.RemoveTmpDirectory();
            TestHelpers.ResetDatabase();

            this._cloudDriveSyncSystemClient1.FileSyncService.StopAllSync();
            this._cloudDriveSyncSystemClient2.FileSyncService.StopAllSync();

            this._cloudDriveSyncSystemClient2.FileSyncService.StopAllSync();
            this._localFileRepositoryService1.Reset();
            this._localFileRepositoryService2.Reset();
            this._cloudDriveSyncSystemClient1.Dispose();
            this._cloudDriveSyncSystemClient2.Dispose();
            this.webApplication.Dispose();
            Thread.Sleep(2000);
            TestHelpers.ResetDatabase();
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
                fileContent,
                0
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
                $"File reposirotry did not reach files amount to one in desired time, exprect to file repository have 1 but has [[{files.Count}]] "
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

            this.CheckIfTheSameContentOnClinetsAndServer(
                new List<CloudDriveSyncSystem>() { this._cloudDriveSyncSystemClient1 }
            );

            this.CheckIfFileContentTheSameAsClientDataBase(
                _localFileRepositoryService1,
                this._Client1Config
            );

            TestHelpers.ResetDatabase();

            #endregion
        }

        [Test]
        public void Create_And_Sync_File_In_EmptyDirectory_With_Antoher_Device_connected()
        {
            #region Ensure connected and empty
            ConnectBothDevices();

            #endregion

            #region Create New File
            string fileContent = "Exmaple File Content_" + Guid.NewGuid();
            String createdFileName = this.AddTMpFiles(1, this._Client1Config).FirstOrDefault();

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
                $"File reposirotry did not reach files amount to one in desired time, exprect to file repository have 1 but has [[{files.Count}]] "
            );

            SyncFileData syncesFile = files.First();

            Assert.That(
                syncesFile.Name == Path.GetFileNameWithoutExtension(createdFileName),
                $"File name in database {syncesFile.Name} is not equal to file name in folder {Path.GetFileNameWithoutExtension(createdFileName)}"
            );

            string serverFileHash = FileManager.GetHashOfFile(
                _fileSystemService.GetFullPathToFile(syncesFile)
            );
            string newDevieFileHAs = "";
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        string newDevieFileHAs = FileManager.GetHashOfFile(
                            _Client2Config.StorageLocation + createdFileName
                        );
                        return serverFileHash.Equals(newDevieFileHAs);
                    });
                },
                $"new device file hash \n[[{newDevieFileHAs}]]\n IS not the same as file hash on server \n[[{serverFileHash}]]\n"
            );

            this.CheckIfTheSameContentOnClinetsAndServer(
                new List<CloudDriveSyncSystem>()
                {
                    this._cloudDriveSyncSystemClient1,
                    this._cloudDriveSyncSystemClient2,
                }
            );

            this.CheckIfFileContentTheSameAsClientDataBase(
                _localFileRepositoryService2,
                this._Client1Config
            );

            // ensure file onwre sahip fo all devices
            Assert.DoesNotThrow(
                (
                    () =>
                    {
                        TestHelpers.EnsureTrue(() =>
                        {
                            return FileRepository
                                    .GetAllUserFiles(UserRepository.getUserByMail(email).id)
                                    .Where(x =>
                                        x.DeviceOwner.Contains(
                                            this._cloudDriveSyncSystemClient1.CredentialManager.GetDeviceID()
                                        )
                                        && x.DeviceOwner.Contains(
                                            this._cloudDriveSyncSystemClient2.CredentialManager.GetDeviceID()
                                        )
                                    )
                                    .Count() == 1;
                        });
                    }
                ),
                $"Files: {String.Join(", ", FileRepository
                .GetAllUserFiles(UserRepository.getUserByMail(this.email).id))} Do not  have both devies as owners"
            );

            #endregion
        }

        private void ConnectBothDevices()
        {
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

            this._cloudDriveSyncSystemClient2.ServerConnection.login(email, pass);
            Assert.That(
                _cloudDriveSyncSystemClient2.ServerConnection.CheckIfAuthirized(),
                $"Client2 is not authorized"
            );

            List<FileData> filesInSyncLocation2 = FileManager.GetAllFilesInLocation(
                _Client2Config.StorageLocation
            );
            Assert.That(
                filesInSyncLocation2.Count == 0,
                "Stating sync2 location is not empty at beginning"
            );

            Assert.That(
                !_cloudDriveSyncSystemClient1
                    .CredentialManager.GetDeviceID()
                    .Equals(_cloudDriveSyncSystemClient2.CredentialManager.GetDeviceID()),
                "Device id should be diffenrt"
            );
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
                        getFileContent(i),
                        i
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

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return this._cloudDriveSyncSystemClient1.ServerConnection.GetListOfFiles().Count
                            == 3;
                    });
                },
                $"User doent have assaigned 3 files but {this._cloudDriveSyncSystemClient1.ServerConnection.GetListOfFiles().Count}"
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
            #region Ensure nor connected and 3 files on server
            Assert.That(
                this._cloudDriveSyncSystemClient1.ServerConnection.CheckIfAuthirized() == false,
                "_cloudDriveSyncSystemClient1 should not be conected to server"
            );
            List<string> newFiles = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                newFiles.Add(AddFileOnServerSide());
            }

            #endregion


            #region Connect to server
            this._cloudDriveSyncSystemClient1.ServerConnection.login(email, pass);
            Assert.That(
                this._cloudDriveSyncSystemClient1.ServerConnection.CheckIfAuthirized(),
                "cloud drive system 1 is no authorized"
            );

            #endregion

            #region Esnure correct file ownership
            Assert.That(
                this._cloudDriveSyncSystemClient1.ServerConnection.GetListOfFiles().Count == 3,
                $"File in server databse not equal to 3"
            );
            Assert.DoesNotThrow(
                () =>
                    TestHelpers.EnsureTrue(() =>
                    {
                        List<SyncFileData> filesOnServer = FileRepository.GetAllUserFiles(
                            UserRepository.getUserByMail(email).id
                        );
                        foreach (SyncFileData syncFileData in filesOnServer)
                        {
                            if (
                                !syncFileData.DeviceOwner.Contains(
                                    this._cloudDriveSyncSystemClient1.CredentialManager.GetDeviceID()
                                )
                            )
                                return false;
                        }

                        return true;
                    }),
                $"Files are not owned by device {this._cloudDriveSyncSystemClient1.CredentialManager.GetDeviceID()}"
            );

            #endregion

            #region Ensure the same data on server and clinet

            this.CheckIfTheSameContentOnClinetsAndServer(
                new List<CloudDriveSyncSystem>() { this._cloudDriveSyncSystemClient1 }
            );
            this.CheckIfFileContentTheSameAsClientDataBase(
                this._localFileRepositoryService1,
                this._Client1Config
            );
            #endregion
        }

        [Test]
        public void Connect_With_Diffrent_Files_On_Server_And_Device()
        {
            #region Ensure diffrent files on server and device
            Assert.That(
                this._cloudDriveSyncSystemClient1.ServerConnection.CheckIfAuthirized() == false,
                "_cloudDriveSyncSystemClient1 should not be conected to server"
            );
            List<string> newFiles = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                newFiles.Add(AddFileOnServerSide());
            }

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
                        getFileContent(i),
                        i
                    )
                );
            }

            #endregion


            #region Connect to server
            this._cloudDriveSyncSystemClient1.ServerConnection.login(email, pass);

            #endregion

            #region Esnure correct file ownership

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return FileRepository
                                .GetAllUserFiles(UserRepository.getUserByMail(email).id)
                                .Count == 6;
                    });
                },
                "Files on server not equal to 6"
            );

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        List<SyncFileData> filesOnServer = FileRepository.GetAllUserFiles(
                            UserRepository.getUserByMail(email).id
                        );
                        foreach (SyncFileData syncFileData in filesOnServer)
                        {
                            if (syncFileData.DeviceOwner.Count == 0)
                            {
                                return false;
                            }
                            if (
                                !syncFileData.DeviceOwner.Contains(
                                    this._cloudDriveSyncSystemClient1.CredentialManager.GetDeviceID()
                                )
                            )
                                return false;
                        }
                        return true;
                    });
                },
                "Files doesn't have owner ship by 2 devices"
            );

            #endregion

            #region Ensure the same data on server and clinet

            this.CheckIfTheSameContentOnClinetsAndServer(
                new List<CloudDriveSyncSystem>() { this._cloudDriveSyncSystemClient1 }
            );

            this.CheckIfFileContentTheSameAsClientDataBase(
                _localFileRepositoryService1,
                this._Client1Config
            );
            #endregion
        }

        [Test]
        public void Delete_File_Located_On_Both_Devices()
        {
            //throw new NotImplementedException("Nor implnted");
            #region Ensure The same file
            this._cloudDriveSyncSystemClient1.ServerConnection.login(email, pass);
            this._cloudDriveSyncSystemClient2.ServerConnection.login(email, pass);

            Assert.That(
                this._cloudDriveSyncSystemClient1.ServerConnection.CheckIfAuthirized(),
                "cloud drive system 1 is no authorized"
            );
            Assert.That(
                this._cloudDriveSyncSystemClient2.ServerConnection.CheckIfAuthirized(),
                "cloud drive system 2 is no authorized"
            );
            Assert.That(
                this._cloudDriveSyncSystemClient1.ServerConnection.WebSocketState
                    == WebSocketState.Open,
                $"_cloudDriveSyncSystemClient1 Conenction not opened but {this._cloudDriveSyncSystemClient1.ServerConnection.WebSocketState}"
            );
            Assert.That(
                this._cloudDriveSyncSystemClient2.ServerConnection.WebSocketState
                    == WebSocketState.Open,
                $"_cloudDriveSyncSystemClient2 Conenction not opened but {this._cloudDriveSyncSystemClient2.ServerConnection.WebSocketState}"
            );
            Assert.That(
                websocketConnectedController.GetAllConnectedDevices().Count() == 2,
                $"Server doenst all have all 2 wbe scoket connetions but [[{websocketConnectedController.GetAllConnectedDevices().Count()}]]"
            );

            int amountOfFiles = 3;
            List<string> filesAdded;
            filesAdded = AddTMpFiles(amountOfFiles, this._Client1Config);
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return FileManager
                                .GetAllFilesInLocation(this._Client1Config.StorageLocation)
                                .Count == 3;
                        ;
                    });
                },
                "Files on device 1 not 3"
            );
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return FileRepository
                                .GetAllUserFiles(UserRepository.getUserByMail(this.email).id)
                                .Where(x =>
                                    x.DeviceOwner.Contains(
                                        this._cloudDriveSyncSystemClient1.CredentialManager.GetDeviceID()
                                    )
                                    && x.DeviceOwner.Contains(
                                        this._cloudDriveSyncSystemClient2.CredentialManager.GetDeviceID()
                                    )
                                )
                                .Count() > 0;
                    });
                },
                $"Files on server repositowry housel have more than 0 file recprd"
            );

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return FileManager
                                .GetAllFilesInLocation(this._Client2Config.StorageLocation)
                                .Count == 3;
                        ;
                    });
                },
                $"Files on device2 not 3 but {FileManager
                    .GetAllFilesInLocation(this._Client2Config.StorageLocation)
                    .Count}"
            );

            Assert.DoesNotThrow(
                () =>
                {
                    this.CheckIfTheSameContentOnClinetsAndServer(
                        new List<CloudDriveSyncSystem>()
                        {
                            this._cloudDriveSyncSystemClient2,
                            this._cloudDriveSyncSystemClient1,
                        }
                    );
                },
                "Files on device and server do not match"
            );

            Assert.DoesNotThrow(
                () =>
                {
                    this.CheckIfFileContentTheSameAsClientDataBase(
                        this._localFileRepositoryService1,
                        this._Client1Config
                    );
                },
                "Files and database on device 1 do not match"
            );
            Assert.DoesNotThrow(
                () =>
                {
                    this.CheckIfFileContentTheSameAsClientDataBase(
                        this._localFileRepositoryService2,
                        this._Client2Config
                    );
                },
                "Files and database on device 2 do not match"
            );

            #endregion

            #region Delete File on one Device
            FileManager.DeleteFile(this._Client1Config.StorageLocation + filesAdded[0]);

            #endregion

            #region Ensure the same data on server and clinet

            #region Both devices should have 2 files
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return FileManager
                                .GetAllFilesInLocation(this._Client1Config.StorageLocation)
                                .Count == 2;
                        ;
                    });
                },
                "Files on device 1 not 2"
            );

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return FileManager
                                .GetAllFilesInLocation(this._Client2Config.StorageLocation)
                                .Count == 2;
                        ;
                    });
                },
                $"Files on device2 not 2 but {FileManager
                    .GetAllFilesInLocation(this._Client2Config.StorageLocation)
                    .Count}"
            );

            #endregion

            #region Files on both devices should be the same


            this.CheckIfTheSameContentOnClinets(
                new List<CloudDriveSyncSystem>()
                {
                    this._cloudDriveSyncSystemClient1,
                    this._cloudDriveSyncSystemClient2,
                }
            );
            #endregion

            #region Files on both devices should be the same as in database

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return this._localFileRepositoryService1.GetAllFiles().Count() == 2;
                    });
                },
                $"File in client databse 1 is not 2\n but {this._localFileRepositoryService1.GetAllFiles().Count()} \n [[{String.Join(", ", this._localFileRepositoryService1.GetAllFiles())}]]"
            );

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(
                        () =>
                        {
                            return this
                                ._localFileRepositoryService1.GetAllFiles()
                                .Order()
                                .SequenceEqual(
                                    this._localFileRepositoryService2.GetAllFiles().Order()
                                );
                        },
                        100000
                    );
                },
                $"Files in local repositories is not the same: \n [[{String.Join(", ", this._localFileRepositoryService1.GetAllFiles())}]]\n!= \n[[{String.Join(", ", this._localFileRepositoryService2.GetAllFiles())}]]\n"
            );

            #endregion

            #region Files on server databse should 3 with one without owners

            Assert.That(
                FileRepository.GetAllUserFiles(UserRepository.getUserByMail(email).id).Count == 4,
                $"File in server database not equal to [[4]] but [[{FileRepository.GetAllUserFiles(UserRepository.getUserByMail(email).id).Count}]] ::: \n {String.Join(", \n", FileRepository.GetAllUserFiles(UserRepository.getUserByMail(email).id))}"
            );
            ;

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(
                        () =>
                        {
                            return FileRepository
                                    .GetAllUserFiles(UserRepository.getUserByMail(email).id)
                                    .Where(x =>
                                        x.DeviceOwner.Contains(
                                            _cloudDriveSyncSystemClient1.CredentialManager.GetDeviceID()
                                        )
                                        && x.DeviceOwner.Contains(
                                            _cloudDriveSyncSystemClient2.CredentialManager.GetDeviceID()
                                        )
                                    )
                                    .Count() == 3;
                        },
                        10000
                    );
                },
                $"Server file repository should heave 3 files with two device owner:: \n {String.Join(", \n", FileRepository.GetAllUserFiles(UserRepository.getUserByMail(email).id))} "
            );
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(
                        () =>
                        {
                            return FileRepository
                                    .GetAllUserFiles(UserRepository.getUserByMail(email).id)
                                    .Where(x => x.DeviceOwner.Count == 0)
                                    .Count() == 1;
                        },
                        10000
                    );
                },
                $"Server file repository should heave one file withou owners but has:: \n {String.Join(", \n", FileRepository.GetAllUserFiles(UserRepository.getUserByMail(email).id))} "
            );

            Console.WriteLine("Test");
            #endregion



            #endregion
        }

        [Test]
        public void Create_File_And_rename_it()
        {
            #region Ensure connected and empty
            ConnectBothDevices();

            #endregion

            #region Create New File
            String createdFileName = this.AddTMpFiles(1, this._Client1Config).FirstOrDefault();

            #endregion

            #region Ensure second device also has file

            TestHelpers.EnsureTrue(() =>
            {
                return _localFileRepositoryService1.GetAllFiles().Count() > 0;
            });

            string serverFclient1 = this
                ._localFileRepositoryService1.GetAllFiles()
                .FirstOrDefault()
                .Hash;
            string newDevieFileHAs = "";
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        string newDevieFileHAs = FileManager.GetHashOfFile(
                            _Client2Config.StorageLocation + createdFileName
                        );
                        return serverFclient1.Equals(newDevieFileHAs);
                    });
                },
                $"new device file hash \n[[{newDevieFileHAs}]]\n IS not the same as file hash on server \n[[{serverFclient1}]]\n"
            );

            #endregion



            #region reNmae New File

            string newName = "newName";
            File.Move(
                $"{_Client1Config.StorageLocation}{createdFileName}",
                $"{_Client1Config.StorageLocation}{newName}.tmp"
            );

            #endregion

            #region Ensure Correct finsihs state


            #region File repository 1 should have new version of file in repository


            Assert.That(
                _localFileRepositoryService1.GetAllFiles().Count() == 1,
                $"Local file repository should only one elements but has {_localFileRepositoryService1.GetAllFiles().Count()}"
            );
            LocalFileData localFileData = null;
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(
                        () =>
                        {
                            localFileData = _localFileRepositoryService1
                                .GetAllFiles()
                                .FirstOrDefault();
                            return localFileData != null && localFileData.Name == newName;
                        },
                        10000
                    );
                },
                $"File name in database [[{_localFileRepositoryService1
                    .GetAllFiles()
                    .FirstOrDefault().Name}]] is not equal to file name in folder [[{newName}]]"
            );

            Assert.That(
                localFileData.Extenstion == ".tmp",
                $"File name in database{localFileData.Extenstion} is not equal to file name in folder {Path.GetExtension(createdFileName)}"
            );
            Assert.That(
                localFileData.Hash
                    == FileManager.GetHashOfFile(_Client1Config.StorageLocation + "newName.tmp"),
                $"File hash in database {localFileData.Hash} is not equal to file hash on disk {FileManager.GetHashOfFile(_Client1Config.StorageLocation + "newName.tmp")}"
            );
            Assert.That(
                localFileData.Version == 1,
                $"New file verion should be one but its [[{localFileData.Version}]]"
            );

            #endregion

            #region Server shoudl 2 file version one with old name one with new name and all devices owners

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return GetAllFilesOnServer().Count == 2;
                    });
                },
                $"File entry on server database should be 2 but there are [[{GetAllFilesOnServer().Count}]]"
            );

            Assert.That(
                GetAllFilesOnServer().Where(x => x.Name == "newName").Count() == 1,
                $"File entry on server database should be 1 with new name but there are [[{GetAllFilesOnServer().Where(x => x.Name == "newName").Count()}]]"
            );

            SyncFileData fileWithNewNameSyncData = GetAllFilesOnServer()
                .FirstOrDefault(x => x.Name == "newName");

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        fileWithNewNameSyncData = GetAllFilesOnServer()
                            .FirstOrDefault(x => x.Name == "newName");
                        return fileWithNewNameSyncData.Version == 1;
                    });
                },
                $"new file should have a new version but has [[{fileWithNewNameSyncData.Version}]] == "
            );

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        fileWithNewNameSyncData = GetAllFilesOnServer()
                            .FirstOrDefault(x => x.Name == "newName");
                        return fileWithNewNameSyncData.DeviceOwner.Count == 2;
                    });
                },
                $"File should have 2 owners but has [[{fileWithNewNameSyncData.DeviceOwner.Count}]]"
            );

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return GetAllFilesOnServer().Where(x => x.DeviceOwner.Count == 0).Count()
                            == 1;
                    });
                },
                $"There should be one file enrtry without device owner but there :: \n [[\n{String.Join(", \n", GetAllFilesOnServer())} \n]]"
            );

            #endregion

            BothDevicesShouldHAveTheSameData();

            #endregion
        }

        private List<string> AddTMpFiles(int amountOfFiles, IConfiguration config)
        {
            List<string> filesAdded;
            var getFileContent = (int num) =>
            {
                return $"Content_{num}";
            };

            filesAdded = new List<string>();
            for (int i = 0; i < amountOfFiles; i++)
            {
                filesAdded.Add(
                    TestHelpers.CreateTmpFile(config.StorageLocation, getFileContent(i), i)
                );
            }

            return filesAdded;
        }

        private void CheckIfTheSameContentOnClinets(
            List<CloudDriveSyncSystem> cloudDriveSyncSystems
        )
        {
            if (cloudDriveSyncSystems.Count <= 1)
            {
                throw new ArgumentException("Cannot compre only one cloud drive system");
            }

            CloudDriveSyncSystem first = cloudDriveSyncSystems.First();
            cloudDriveSyncSystems.Remove(first);
            foreach (CloudDriveSyncSystem cloudDriveSyncSystemToCopmpare in cloudDriveSyncSystems)
            {
                List<UploudFileData> filesInFirst = FileManager.GetUploadFileDataInLocation(
                    first.Configuration.StorageLocation
                );
                List<UploudFileData> filesInCompare = FileManager.GetUploadFileDataInLocation(
                    cloudDriveSyncSystemToCopmpare.Configuration.StorageLocation
                );
                Assert.That(
                    filesInFirst.SequenceEqual(filesInCompare),
                    $"FIle in device {first.CredentialManager.GetDeviceID()} Not the same as id device {cloudDriveSyncSystemToCopmpare.CredentialManager.GetDeviceID()}\n \n---------\n\n [[\n{String.Join(", \n", filesInFirst)}\n]]\n\n !=\n\n [[\n{String.Join(", \n", filesInCompare)}\n]]\n\n"
                );
            }
        }

        #region HelpersMethod

        private string AddFileToBothServerAndClients()
        {
            throw new NotImplementedException("AddFileToBothServerAndClients not implemented");
        }

        private void CheckIfFileContentTheSameAsClientDataBase(
            IFileRepositoryService localFileRepositoryService,
            IConfiguration configuration
        )
        {
            List<FileData> filesInSyncLocation = new List<FileData>();
            IEnumerable<UploudFileData> fileInDatabase = new List<UploudFileData>();
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        filesInSyncLocation = FileManager.GetAllFilesInLocation(
                            configuration.StorageLocation
                        );
                        fileInDatabase = localFileRepositoryService.GetAllFiles();
                        return fileInDatabase.Count() == filesInSyncLocation.Count;
                    });
                },
                $"Files in sync location and database are not the same [[{filesInSyncLocation.Count}]] != [[{fileInDatabase.Count()}]]"
            );

            foreach (UploudFileData uploudFileData in fileInDatabase)
            {
                UploudFileData mathingFile = fileInDatabase.First(x =>
                    x.GetRealativePath().Equals(uploudFileData.GetRealativePath())
                );
                string filename = mathingFile.getFullFilePathForBasePath(
                    configuration.StorageLocation
                );
                string localFileHash = FileManager.GetHashOfFile(filename);
                Assert.That(
                    localFileHash.Equals(uploudFileData.Hash),
                    $"File hash in database {uploudFileData.Hash} is not the same as file hash on disk {localFileHash}"
                );
            }
        }

        private string AddFileOnServerSide()
        {
            Guid guid = Guid.NewGuid();
            long userID = UserRepository.getUserByMail(email).id;
            MemoryStream memoryStream = new MemoryStream();
            string content = "Example file content______" + Guid.NewGuid();
            memoryStream.Write(
                Encoding.ASCII.GetBytes(content),
                0,
                Encoding.ASCII.GetByteCount(content)
            );
            memoryStream.Position = 0;
            string newfileName = Path.GetFileName(Path.GetTempFileName());
            TestHelpers
                .GetDeafultFileSystemService()
                .SaveFile($"{userID}//{guid.ToString()}", memoryStream);

            FileRepository.SaveNewFile(
                new SyncFileData()
                {
                    Name = Path.GetFileNameWithoutExtension(newfileName),
                    Extenstion = Path.GetExtension(newfileName),
                    Path = ".",
                    OwnerId = userID,
                    Hash = FileManager.getHashOfArrayBytes(memoryStream.ToArray()),
                    Id = guid,
                    Version = 0,
                    DeviceOwner = new List<string>(),
                }
            );
            return newfileName;
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
                    $"Files on device {cloudDriveSyncSystem.CredentialManager.GetDeviceID()}"
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
                    string filepath = correspoidingFile.getFullFilePathForBasePath(
                        cloudDriveSyncSystem.Configuration.StorageLocation
                    );
                    string loacalHash = FileManager.GetHashOfFile(filepath);
                    Assert.That(
                        loacalHash == serverFileHash,
                        "Content of file {correspoidingFile} diffrent from server and device"
                    );
                }
            }
        }

        List<SyncFileData> GetAllFilesOnServer()
        {
            return FileRepository.GetAllUserFiles(UserRepository.getUserByMail(email).id);
        }

        void BothDevicesShouldHAveTheSameData()
        {
            Assert.That(
                this._localFileRepositoryService1.GetAllFiles()
                    .Order()
                    .SequenceEqual(this._localFileRepositoryService2.GetAllFiles().Order()),
                $"Both local repositories should bew equal but are \n [[{String.Join(", \n", this._localFileRepositoryService1.GetAllFiles())}  \n]] \n != \n[[{String.Join(", \n", this._localFileRepositoryService2.GetAllFiles())} \n]]"
            );

            List<UploudFileData> filesOnDevice1 = FileManager.GetUploadFileDataInLocation(
                this._Client1Config.StorageLocation
            );
            List<UploudFileData> filesOnDevice2 = FileManager.GetUploadFileDataInLocation(
                this._Client2Config.StorageLocation
            );

            Assert.That(
                filesOnDevice1.Order().SequenceEqual(filesOnDevice1.Order()),
                $"Both local repositories should bew equal but are \n [[{String.Join(", \n", filesOnDevice1)}  \n]] \n != \n[[{String.Join(", \n", filesOnDevice1)} \n]]"
            );
        }

        #endregion
    }
}
