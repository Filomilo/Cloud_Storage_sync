using NUnit.Framework;
using Cloud_Storage_Desktop_lib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Desktop_lib.Services.Tests
{
    [TestFixture()]
    public class CredentialManagerTests
    {
        [Test()]
        public void SaveTokenTest()
        {
            CredentialManager.removeToken();
            string uuuid = Guid.NewGuid().ToString();
            Assert.That(CredentialManager.GetToken()=="");
            CredentialManager.SaveToken(uuuid);
            Assert.That(CredentialManager.GetToken() == uuuid);
            CredentialManager.removeToken();
            Assert.That(CredentialManager.GetToken() == "");
        }
    }
}