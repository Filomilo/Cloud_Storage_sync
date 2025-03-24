using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Server;
using Cloud_Storage_Server.Controllers;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
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
        private ServerConnection server;

        [SetUp]
        public void Setup()
        {
            _testServer = new MyWebApplication().CreateDefaultClient();
            this.server = new ServerConnection(_testServer);

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
            ServerConnection server = new ServerConnection(_testServer);
            Assert.That(server.CheckIfHelathy());
        }

        [Test()]
        public void ServerConnectionTest_incorrect()
        {
            Assert.Throws(
                Is.TypeOf(typeof(AggregateException)),
                () =>
                {
                    ServerConnection server = new ServerConnection("http://localhost:1234");
                }
            );
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
            List<UploudFileData> files = FileManager.GetAllFilesInLocation(
                TestHelpers.ExampleDataDirectory
            );
            files.ForEach(x =>
                x.Path = Path.GetRelativePath(TestHelpers.ExampleDataDirectory, x.Path)
            );
            List<byte[]> bytArrays = new List<byte[]>();
            foreach (UploudFileData file in files)
            {
                byte[] data = File.ReadAllBytes(
                    file.getFullPathForBasePath(TestHelpers.ExampleDataDirectory)
                );
                bytArrays.Add(data);
                Assert.DoesNotThrow(() =>
                {
                    this.server.uploudFile(file, data);
                });
            }

            List<FileData> filesOnServer = this.server.GetListOfFiles();
            Assert.That(filesOnServer.Count == files.Count);
            for (var i = 0; i < filesOnServer.Count; i++)
            {
                UploudFileData uploudFileData = files.First(x => x.Hash == filesOnServer[i].Hash);
                Assert.That(uploudFileData.Name == filesOnServer[i].Name);
                Assert.That(uploudFileData.Path == filesOnServer[i].Path);
                Assert.That(uploudFileData.Extenstion == filesOnServer[i].Extenstion);
                byte[] bytes = this.server.DownloadFlie(filesOnServer[i].Id);
                Assert.That(Enumerable.SequenceEqual(bytes, bytArrays[i]));
                string hash = FileManager.getHashOfArrayBytes(bytes);

                Assert.That(filesOnServer[i].Hash == hash);
            }
        }
    }
}
