using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_Desktop_lib.Tests;
using Cloud_Storage_Server.Controllers;
using Cloud_Storage_Server.Database.Models;
using NUnit.Framework;

namespace Cloud_Storage_Server.Controllers.Tests
{
    [TestFixture()]
    public class FilesControllerTests
    {
        private string exampleDataImageDirector =
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
            + "testData\\nyan.jpg";

        private HttpClient testServer;

        [SetUp]
        public void Setup()
        {
            testServer = new MyWebApplication().CreateDefaultClient();

            string email = $"{Guid.NewGuid().ToString()}@mail.mail";
            string pass = "1234567890asdASD++";
            string token = testServer
                .PostAsJsonAsync(
                    "/api/Auth/Register",
                    new AuthRequest() { Email = email, Password = pass }
                )
                .Result.Content.ReadAsStringAsync()
                .Result;
            testServer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token
            );
        }

        [TearDown]
        public void tearDown()
        {
            testServer.Dispose();
        }

        [Test()]
        public void UploudFileTest()
        {
            Stream exmpaleData = FileManager.GetStreamForFile(exampleDataImageDirector);

            var form = FileMangamentSerivce.GetFormDatForFile(
                new SyncFileData()
                {
                    Name = "file",
                    Extenstion = "jpg",
                    Hash = "123",
                    Id = new Guid(),
                    Owner = null,
                    OwnerId = 2,
                    Path = "folder\\23456",
                    SyncDate = new DateTime(),
                },
                exmpaleData
            );
            var response = testServer.PostAsync("api/Files/upload", form).Result;
            Assert.That(response.IsSuccessStatusCode);
        }
    }
}
