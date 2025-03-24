using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Test]
        public void GetFileTest()
        {
            string example_subdir = $"exmpale\\example\\example\\2\\{Guid.NewGuid()}.jpg";
            byte[] exmpaleData = File.ReadAllBytes(TestHelpers.ExampleDataDirectory + "//nyan.jpg");
            this.iFileSystemService.SaveFile(example_subdir, exmpaleData);
            byte[] readBytes = this.iFileSystemService.GetFile(example_subdir);
            Assert.That(readBytes.SequenceEqual(exmpaleData));
            this.iFileSystemService.DeleteFile(example_subdir);
            Assert.Throws(
                Is.InstanceOf(typeof(Exception)),
                () =>
                {
                    this.iFileSystemService.GetFile(example_subdir);
                }
            );
        }
    }
}
