using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Test;
using NUnit.Framework;

namespace Cloud_Storage_Desktop_lib.Tests
{
    [TestFixture()]
    public class CloudDriveSyncSystemTests
    {
        private CloudDriveSyncSystem _cloudDriveSyncSystem;

        private HttpClient _testServer;

        [SetUp]
        public void Setup()
        {
            _testServer = new MyWebApplication().CreateDefaultClient();
            this._cloudDriveSyncSystem = new CloudDriveSyncSystem(_testServer);

            string email = $"{Guid.NewGuid().ToString()}@mail.mail";
            string pass = "1234567890asdASD++";
            _cloudDriveSyncSystem.ServerConnection.Register(email, pass);
            Assert.That(_cloudDriveSyncSystem.ServerConnection.CheckIfAuthirized());
        }

        [TearDown]
        public void tearDown()
        {
            if (Directory.Exists(TestHelpers.TmpDirecotry))
            {
                Directory.Delete(TestHelpers.TmpDirecotry, true);
            }
            _testServer.Dispose();
        }

        [Test()]
        [Ignore("Depracted")]
        public void UploudDownloadFilesTest()
        {
            //CloudDriveSyncSystem.Instance.Configuration.StorageLocation =
            //    TestHelpers.ExampleDataDirectory;
            //Assert.DoesNotThrow(() =>
            //{
            //    CloudDriveSyncSystem.Instance.UploudFiles();
            //});
            //List<UploudFileData> fileInLocaiton = FileManager.GetAllFilesInLocationRelative(
            //    TestHelpers.ExampleDataDirectory
            //);
            //List<SyncFileData> files = CloudDriveSyncSystem.Instance.GetListOfFilesOnCloud();
            //Assert.That(fileInLocaiton.Count == files.Count);

            //CloudDriveSyncSystem.Instance.Configuration.StorageLocation = TestHelpers.TmpDirecotry;
            //CloudDriveSyncSystem.Instance.DownloadFiles();

            //List<UploudFileData> savedFiles = FileManager.GetAllFilesInLocationRelative(
            //    TestHelpers.TmpDirecotry
            //);

            //foreach (UploudFileData uploudFileData in fileInLocaiton)
            //{
            //    UploudFileData match = savedFiles.First(x => x.Name == uploudFileData.Name);
            //    Assert.That(match != null);
            //    Assert.That(match.Hash == uploudFileData.Hash);
            //    Assert.That(match.Extenstion == uploudFileData.Extenstion);
            //    Assert.That(match.Path == uploudFileData.Path);
            //}
        }
    }
}
