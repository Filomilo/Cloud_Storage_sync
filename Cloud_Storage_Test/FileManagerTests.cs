using NUnit.Framework;
using Cloud_Storage_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Cloud_Storage_Test;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Common.Tests
{
    [TestFixture()]
    public class FileManagerTests
    {
        [Test()]
        public void GetAllFilesInLocationTest()
        {
            List<UploudFileData> files = Cloud_Storage_Common.FileManager.GetAllFilesInLocation(TestHelpers.ExampleDataDirectory);
            Assert.That(files.Count==3);

        }
    }
}