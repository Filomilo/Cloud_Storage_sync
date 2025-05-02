using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Cloud_Storage_Test
{
    [TestFixture]
    public class TestTestServer
    {
        [Test]
        public void TestStartAndConnectionToserverITestEnverment()
        {
            ServerControlHelpers.Instance.StartServer();

            var client = new HttpClient
            {
                BaseAddress = new Uri(ServerControlHelpers.Instance.GetIpConnection()),
            };
            String res = client
                .GetAsync("api/Helath/health")
                .Result.Content.ReadAsStringAsync()
                .Result;
            Assert.That(res != null);
            Assert.That(res.Equals("healthy"));
        }
    }
}
