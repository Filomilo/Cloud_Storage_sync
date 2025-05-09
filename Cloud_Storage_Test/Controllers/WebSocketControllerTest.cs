﻿using System.Net.WebSockets;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Tests;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Cloud_Storage_Test.Controllers
{
    [TestFixture]
    class WebSocketControllerTest
    {
        private WebSocketClient webSocketClient;
        private HttpClient _testServer;
        private TestCredentialMangager credentialMangager = new TestCredentialMangager();
        private IWebSocketWrapper webSocketWrapper;
        private MyWebApplication webApplication;
        private string email = "";
        string pass = "1234567890asdASD++";
        private IWebsocketConnectedController websocketConnectedController;

        [SetUp]
        public void Setup()
        {
            TestHelpers.RemoveTmpDirectory();
            ILogger logger = CloudDriveLogging.Instance.GetLogger("TestLogging");
            CloudDriveLogging.Instance.SetTestLogger(logger);
            webApplication = new MyWebApplication();
            _testServer = webApplication.CreateDefaultClient();
            webSocketClient = webApplication.Server.CreateWebSocketClient();
            email = $"{Guid.NewGuid().ToString()}@mail.mail";
            webSocketWrapper = new TestWebScoket(webSocketClient);
            websocketConnectedController = (IWebsocketConnectedController)
                webApplication.Server.Services.GetService(typeof(IWebsocketConnectedController));
        }

        //[TearDown]
        //public void TearDown()
        //{
        //    TestHelpers.RemoveTmpDirectory();
        //}


        private static WebSocketMessage[] TestedMessages = new WebSocketMessage[]
        {
            new WebSocketMessage("TestMEssage"),
            new WebSocketMessage(
                new UpdateFileDataRequest()
                {
                    DeviceReuqested = "123",
                    newFileData = new SyncFileData()
                    {
                        Extenstion = ".mp4",
                        Hash = "123",
                        Name = "Name",
                        Path = ".",
                        Version = 1,
                        DeviceOwner = new List<string>() { "123" },
                        Id = Guid.NewGuid(),
                        OwnerId = 1,
                    },
                    oldFileData = null,
                    UserID = 1,
                }
            ),
        };

        [Test]
        public void testConnectToWebScoket(
            [ValueSource("TestedMessages")] WebSocketMessage webSocketMessage
        )
        {
            bool recivedMEssage = false;

            IServerConnection serverConnection = new ServerConnection(
                this._testServer,
                credentialMangager,
                this.webSocketWrapper
            );
            serverConnection.ServerWerbsocketHadnler += (WebSocketMessage message) =>
            {
                Assert.That(message.Equals(webSocketMessage));
                recivedMEssage = true;
            };
            serverConnection.Register(email, pass);
            Assert.That(serverConnection.CheckIfAuthirized());
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return this.webSocketWrapper.State == WebSocketState.Open;
                    });
                },
                $"Web socket connection not opened but {this.webSocketWrapper.State}"
            );

            Assert.That(
                websocketConnectedController.GetAllConnectedDevices().Count() == 1,
                $"Connected deviceds count is not 1 but {websocketConnectedController.GetAllConnectedDevices().Count()}"
            );
            using (var context = new SqliteDataBaseContextGenerator().GetDbContext())
            {
                websocketConnectedController.SendMessageToUser(
                    UserRepository.getUserByMail(context, email).id,
                    webSocketMessage
                );
            }

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return recivedMEssage;
                    });
                },
                $"Message not recived by user {email} but {webSocketMessage}"
            );
        }
    }
}
