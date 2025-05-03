using Cloud_Storage_Desktop_lib.Interfaces;
using NUnit.Framework;

namespace Cloud_Storage_Desktop_lib.Services.Tests
{
    [TestFixture()]
    public class CredentialManagerTests
    {
        private ICredentialManager CredentialManager =
            CredentialManageFactory.GetCredentialManager();

        [Test()]
        public void SaveTokenTest()
        {
            CredentialManager.RemoveToken();
            string uuuid = Guid.NewGuid().ToString();
            Assert.That(CredentialManager.GetToken() == "");
            CredentialManager.SaveToken(uuuid);
            Assert.That(CredentialManager.GetToken() == uuuid);
            CredentialManager.RemoveToken();
            Assert.That(CredentialManager.GetToken() == "");
        }
    }
}
