using NUnit.Framework;
using Cloud_Storage_Server.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Tests;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Services;
using System.Net.Http.Headers;
using System.Xml.Linq;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;

namespace Cloud_Storage_Server.Controllers.Tests
{
    [TestFixture()]
    public class FilesControllerTests
    {

        private string exampleDataImageDirector = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "testData\\nyan.jpg";


        private HttpClient testServer;
        
        [SetUp]
        public void Setup()
        {

            testServer = new MyWebApplication().CreateDefaultClient();
  
            string email = $"{Guid.NewGuid().ToString()}@mail.mail";
            string pass = "1234567890asdASD++";
            string token = testServer.PostAsJsonAsync("/api/Auth/Register", new AuthRequest(){Email = email,Password = pass}).Result.Content.ReadAsStringAsync().Result;
            testServer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        }
        [TearDown]
        public void tearDown()
        {
            testServer.Dispose();
        }


        [Test()]
        public void UploudFileTest()
        {
           

            byte[] exmpaleData = File.ReadAllBytes(exampleDataImageDirector);
          
            var form= FileMangamentSerivce.GetFormDatForFile(new FileData()
            {
                Name = "file",
                Extenstion = "jpg",
                Hash = new byte[] { },
                Id = new Guid(),
                Owner = null,
                OwnerId = 2,
                Path = "\\",
                SyncDate = new DateTime()
            },exmpaleData);
                var response = testServer.PostAsync("api/Files/uploud", form).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Student data uploaded successfully!");
                }
                else
                {
                    Console.WriteLine($"Failed to upload data: {response.StatusCode}");
                }
            }
        }
    }
