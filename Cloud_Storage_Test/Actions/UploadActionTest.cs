using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Test;
using NUnit.Framework;

namespace Cloud_Storage_Desktop_lib.Actions
{
    [TestFixture]
    class UploadActionTest
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
        }

        [Test]
        public void Upload_ActionTest()
        {
            List<SyncFileData> filesFromSererBefore = server.GetListOfFiles();
            Assert.That(filesFromSererBefore.Count == 0);
            Configuration.StorageLocation = TestHelpers.ExampleDataDirectory;
            List<FileData> files = FileManager.GetAllFilesInLocation(
                TestHelpers.ExampleDataDirectory
            );
            foreach (FileData fileData in files)
            {
                UploudFileData file = FileManager.GetUploadFileData(
                    fileData.getFullFilePathForBasePath(Configuration.StorageLocation),
                    Configuration.StorageLocation
                );
                UploadAction uploadAction = new UploadAction(
                    this.server,
                    this.Configuration,
                    new Services.FileRepositoryService(new TestDbContextGenerator1()),
                    file
                );
                Assert.DoesNotThrow(() =>
                {
                    uploadAction.ActionToRun.Invoke();
                });
            }

            List<SyncFileData> filesFromSerer = server.GetListOfFiles();
            Assert.That(filesFromSerer.Count == files.Count);
        }
    }
}
