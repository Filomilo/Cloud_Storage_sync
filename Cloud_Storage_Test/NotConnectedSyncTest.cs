using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using NUnit.Framework;

namespace Cloud_Storage_Test
{
    [TestFixture]
    public class NotConnectedSyncTest
    {
        private string syncPAth;
        private IFileRepositoryService fileRepositoryService;

        [SetUp]
        public void Setup()
        {
            syncPAth = TestHelpers.GetNewTmpDir(
                "NoConneciotnTests" + Guid.NewGuid().ToString().Split("-")[0],
                true
            );
            CloudDriveSyncSystem.Instance.Configuration.StorageLocation = syncPAth;
            CloudDriveSyncSystem.Instance.Configuration.ApiUrl = "http://localhost/";
            CloudDriveSyncSystem.Instance.Configuration.SaveConfiguration();
            CloudDriveSyncSystem.Instance.Configuration.LoadConfiguration();
            fileRepositoryService = CloudDriveSyncSystem.Instance._FileRepositoryService;

            Assert.That(
                !this.fileRepositoryService.GetAllFiles().Any(),
                $"Inital databse not 0::\n {String.Join(",\n", this.fileRepositoryService.GetAllFiles())}"
            );
        }

        [TearDown]
        public void Teardown()
        {
            CloudDriveSyncSystem.Instance.FileSyncService.StopAllSync();
            fileRepositoryService.Reset();
            TestHelpers.RemoveTmpDirectory();
        }

        [Test]
        public void AddFileWithoutServerConnected()
        {
            String createdFileName = TestHelpers.CreateTmpFile(this.syncPAth, "TestFile", 1);
            Assert.DoesNotThrow(
                (
                    () =>
                    {
                        TestHelpers.EnsureTrue(
                            (
                                () =>
                                {
                                    return fileRepositoryService.GetAllFiles().Count() == 1;
                                }
                            )
                        );
                    }
                )
            );

            Assert.That(
                fileRepositoryService.GetAllFiles().ToArray()[0].GetFileNameANdExtenstion()
                    == createdFileName
            );
        }

        [Test]
        public void ChangeStorageLocationToLocationWithFiles()
        {
            string Location = TestHelpers.GetNewTmpDir("newSyncLocationWIthFils", true);
            List<string> NewlyCreatedFiles = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                NewlyCreatedFiles.Add(TestHelpers.CreateTmpFile(Location, $"Test{i}", i));
            }

            CloudDriveSyncSystem.Instance.Configuration.StorageLocation = Location;
            CloudDriveSyncSystem.Instance.Configuration.SaveConfiguration();
            Assert.DoesNotThrow(
                (
                    () =>
                    {
                        TestHelpers.EnsureTrue(
                            (
                                () =>
                                {
                                    return fileRepositoryService.GetAllFiles().Count()
                                        == NewlyCreatedFiles.Count();
                                }
                            )
                        );
                    }
                ),
                $"Local file repository doesnt have files in new Configuraiton:: \n Respository: \n [[\n{String.Join(", \n", fileRepositoryService.GetAllFiles())}\n]], \n localyCrated: \n[[\n {String.Join(", \n", NewlyCreatedFiles)}\n]]"
            );
        }

        [Test]
        public void EmptyDataBaseAfterChangingSyncLocaiton()
        {
            String createdFileName = TestHelpers.CreateTmpFile(this.syncPAth, "TestFile", 1);
            Assert.DoesNotThrow(
                (
                    () =>
                    {
                        TestHelpers.EnsureTrue(
                            (
                                () =>
                                {
                                    return fileRepositoryService.GetAllFiles().Count() == 1;
                                }
                            )
                        );
                    }
                )
            );
            string newPathToSync = TestHelpers.GetNewTmpDir("newSyncPAth", true);
            CloudDriveSyncSystem.Instance.Configuration.StorageLocation = newPathToSync;
            CloudDriveSyncSystem.Instance.Configuration.SaveConfiguration();
            Assert.DoesNotThrow(
                (
                    () =>
                    {
                        TestHelpers.EnsureTrue(
                            (
                                () =>
                                {
                                    return fileRepositoryService.GetAllFiles().Any() == false;
                                }
                            )
                        );
                    }
                ),
                $"File reposiotry not empty after cahngi sync locaiton"
            );
        }
    }
}
