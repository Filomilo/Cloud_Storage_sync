using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Server.Services;
using Cloud_Storage_Test;
using NUnit.Framework;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Cloud_Storage_Server.Services.Tests
{
    [TestFixture]
    public class FileSystemServiceTests
    {
        private IFileSystemService iFileSystemService = new FileSystemService(
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "tmp\\"
        );
    }
}
