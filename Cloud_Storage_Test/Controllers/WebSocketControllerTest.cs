using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_Desktop_lib.Tests;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

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

        [Test]
        public void testConnectToWebScoket()
        {
            bool recivedMEssage = false;
            WebSocketMessage webSocketMessage = new WebSocketMessage("TestMEssage");

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

            websocketConnectedController.SendMessageToUser(
                UserRepository.getUserByMail(email).id,
                webSocketMessage
            );
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
