using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_Test;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Cloud_Storage_Desktop_lib.Tests
{
    class MyWebApplication : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            return base.CreateHost(builder);
        }
    }

    [TestFixture()]
    public class ServerConnectionTest
    {
        private HttpClient _testServer;
        private IServerConnection server;

        [SetUp]
        public void Setup()
        {
            _testServer = new MyWebApplication().CreateDefaultClient();
            this.server = new ServerConnection(
                _testServer,
                new TestCredentialMangager(),
                new WebSocketWrapper()
            );

            string email = $"{Guid.NewGuid().ToString()}@mail.mail";
            string pass = "1234567890asdASD++";
            server.Logout();
            Assert.That(server.CheckIfAuthirized() == false);
            server.Register(email, pass);
            Assert.That(server.CheckIfAuthirized());
        }

        [TearDown]
        public void tearDown()
        {
            _testServer.Dispose();
        }

        [Test()]
        public void ServerConnectionTest_correct()
        {
            IServerConnection server = new ServerConnection(
                _testServer,
                new TestCredentialMangager(),
                new WebSocketWrapper()
            );
            Assert.That(server.CheckIfHelathy());
        }

        [Test()]
        public void ServerConnectionTest_incorrect()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ServerConnection server = new ServerConnection(
                    "http://localhost:1234",
                    new TestCredentialMangager(),
                    new WebSocketWrapper()
                );
            });
        }

        [Test()]
        public void loginTest()
        {
            server.Logout();
            string email = $"{Guid.NewGuid().ToString()}@mail.mail";
            string pass = "1234567890asdASD++";
            server.Logout();
            Assert.That(server.CheckIfAuthirized() == false);
            server.Register(email, pass);
            Assert.That(server.CheckIfAuthirized());
            server.Logout();
            Assert.That(server.CheckIfAuthirized() == false);
            server.login(email, pass);
            Assert.That(server.CheckIfAuthirized());
            server.Logout();
            Assert.That(server.CheckIfAuthirized() == false);
        }

        private string exampleDataImageDirector =
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
            + "testData\\nyan.jpg";

        [Test]
        public void uploudAndDownloadFile()
        {
            List<FileData> files = FileManager.GetAllFilesInLocation(
                TestHelpers.ExampleDataDirectory
            );
            List<UploudFileData> uploudFiles = new List<UploudFileData>();
            files.ForEach(x =>
                x.Path = Path.GetRelativePath(TestHelpers.ExampleDataDirectory, x.Path)
            );
            List<byte[]> bytArrays = new List<byte[]>();
            foreach (FileData file in files)
            {
                string fullFilePath = file.getFullFilePathForBasePath(
                    TestHelpers.ExampleDataDirectory
                );

                UploudFileData uploudFileData = FileManager.GetUploadFileData(
                    fullFilePath,
                    TestHelpers.ExampleDataDirectory
                );
                using (Stream stream = FileManager.GetStreamForFile(fullFilePath))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer);
                    bytArrays.Add(buffer);
                }

                using (Stream stream = FileManager.GetStreamForFile(fullFilePath))
                {
                    uploudFiles.Add(uploudFileData);
                    Assert.DoesNotThrow(() =>
                    {
                        this.server.UploudFile(uploudFileData, stream);
                    });
                }
            }

            List<SyncFileData> filesOnServer = this.server.GetListOfFiles();
            Assert.That(filesOnServer.Count == files.Count);
            for (var i = 0; i < filesOnServer.Count; i++)
            {
                UploudFileData uploudFileData = uploudFiles.First(x =>
                    x.Hash == filesOnServer[i].Hash
                );
                Assert.That(uploudFileData.Name == filesOnServer[i].Name);
                Assert.That(uploudFileData.Path == filesOnServer[i].Path);
                Assert.That(uploudFileData.Extenstion == filesOnServer[i].Extenstion);
                Stream stream = this.server.DownloadFile(filesOnServer[i].Id);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes);
                Assert.That(Enumerable.SequenceEqual(bytes, bytArrays[i]));

                string hash = FileManager.getHashOfArrayBytes(bytes);
                Assert.That(filesOnServer[i].Hash.Equals(hash));
            }
        }
    }
}
