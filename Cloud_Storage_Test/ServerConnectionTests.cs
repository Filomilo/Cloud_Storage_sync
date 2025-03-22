using NUnit.Framework;
using Cloud_Storage_Desktop_lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Cloud_Storage_Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

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
           this.server= new ServerConnection(_testServer);

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
            Assert.Throws(Is.TypeOf(typeof(AggregateException)), () =>
                {
                    ServerConnection server = new ServerConnection("http://localhost:1234");
                }
            );
        }

        [Test()]
        public void loginTest()
        {
         
            string email = $"{Guid.NewGuid().ToString()}@mail.mail";
            string pass = "1234567890asdASD++";
            server.Logout();
            Assert.That(server.CheckIfAuthirized()==false);
            server.Register(email, pass);
            Assert.That(server.CheckIfAuthirized());
            server.Logout();
            Assert.That(server.CheckIfAuthirized() == false);
            server.login(email, pass);
            Assert.That(server.CheckIfAuthirized());
            server.Logout();
            Assert.That(server.CheckIfAuthirized() == false);
        }
    }
}