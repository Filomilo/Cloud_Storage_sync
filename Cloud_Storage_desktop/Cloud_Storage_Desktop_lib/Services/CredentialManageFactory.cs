using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib.Interfaces;

namespace Cloud_Storage_Desktop_lib.Services
{
    public enum CREDENTIALMANGGETYPE
    {
        FILEMAGNE,
        WINDOWS_CREDETNIAL,
        INMEMORY_CREDENTIALS,
    }

    public static class CredentialManageFactory
    {
        public static CREDENTIALMANGGETYPE type = CREDENTIALMANGGETYPE.FILEMAGNE;

        public static ICredentialManager GetCredentialManager()
        {
            switch (type)
            {
                case CREDENTIALMANGGETYPE.FILEMAGNE:
                    return new CredentialManager();
                case CREDENTIALMANGGETYPE.WINDOWS_CREDETNIAL:
                    return new WindwosManager();
                case CREDENTIALMANGGETYPE.INMEMORY_CREDENTIALS:
                    return new InMemoryCredentialMangerL();
            }

            throw new ArgumentException("Type nothadnled");
        }
    }
}
