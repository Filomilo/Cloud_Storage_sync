using NUnit.Framework;

namespace Cloud_Storage_Test
{
    [TestFixture]
    public class TestTestServer
    {
        [TearDown]
        public void tearDown()
        {
            ServerControlHelpers.Instance.StopServer();
        }

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
