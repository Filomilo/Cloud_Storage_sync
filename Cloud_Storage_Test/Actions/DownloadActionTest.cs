using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using NUnit.Framework;

namespace Cloud_Storage_Test.Actions
{
    [TestFixture]
    class DownloadActionTest
    {
        private IServerConnection server;
        public IConfiguration Configuration;

        [SetUp]
        public void Setup()
        {
            server = TestHelpers.getTestServerConnetion();
            Configuration = TestHelpers.GetTestConfig();

            string email = $"{Guid.NewGuid().ToString()}@mail.mail";
            string pass = "1234567890asdASD++";
            server.Logout();
            Assert.That(server.CheckIfAuthirized() == false);
            server.Register(email, pass);
            Assert.That(server.CheckIfAuthirized());
            TestHelpers.UploudAccontDataToLoggedUser(
                server,
                this.Configuration,
                new Cloud_Storage_Desktop_lib.Services.FileRepositoryService(
                    new TestDbContextGenerator1()
                )
            );
            if (Directory.Exists(TestHelpers.TmpDirecotry))
                Directory.Delete(TestHelpers.TmpDirecotry, true);
            this.Configuration.StorageLocation = TestHelpers.TmpDirecotry;
        }

        [TearDown]
        public void tearDown()
        {
            if (Directory.Exists(TestHelpers.TmpDirecotry))
                Directory.Delete(TestHelpers.TmpDirecotry, true);
        }

        [Test]
        public void Download_ActionTest()
        {
            List<SyncFileData> filesToDownload = server.GetListOfFiles();
            List<FileData> FilesInExampleDir = FileManager.GetAllFilesInLocation(
                TestHelpers.ExampleDataDirectory
            );
            List<UploudFileData> caluclatedExampleFiles = new List<UploudFileData>();
            foreach (FileData fileData in FilesInExampleDir)
            {
                caluclatedExampleFiles.Add(
                    FileManager.GetUploadFileData(
                        fileData.getFullFilePathForBasePath(TestHelpers.ExampleDataDirectory),
                        TestHelpers.ExampleDataDirectory
                    )
                );
            }

            Assert.That(filesToDownload.Count == FilesInExampleDir.Count);

            foreach (SyncFileData syncFileData in filesToDownload)
            {
                DownloadAction downloadAction = new DownloadAction(
                    this.server,
                    this.Configuration,
                    syncFileData,
                    new FileRepositoryService(new TestDbContextGenerator1())
                );
                downloadAction.ActionToRun.Invoke();
            }

            List<UploudFileData> downloadedFiles = FileManager.GetUploadFileDataInLocation(
                TestHelpers.TmpDirecotry
            );

            Assert.That(downloadedFiles.Count == filesToDownload.Count);
            foreach (UploudFileData uploud in filesToDownload)
            {
                UploudFileData synced = null;
                Assert.DoesNotThrow(() =>
                {
                    synced = downloadedFiles.Find(x => x.Hash.Equals(uploud.Hash));
                });
                Assert.That(synced != null);
                Assert.That(synced.Name == uploud.Name);
                Assert.That(synced.Path == uploud.Path);
                Assert.That(synced.Extenstion == uploud.Extenstion);
            }
        }
    }
}
