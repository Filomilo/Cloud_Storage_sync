using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Test;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using NUnit.Framework;

namespace Cloud_Storage_Common.Tests
{
    [TestFixture()]
    public class FileManagerTests
    {
        [Test()]
        public void GetAllFilesInLocationTest()
        {
            List<FileData> files = Cloud_Storage_Common.FileManager.GetAllFilesInLocation(
                TestHelpers.ExampleDataDirectory
            );
            Assert.That(files.Count == 3);
        }

        [Test()]
        public void GetAllFilesInLocationRelativeTest()
        {
            List<FileData> files = Cloud_Storage_Common.FileManager.GetAllFilesInLocationRelative(
                TestHelpers.ExampleDataDirectory
            );
            Assert.That(files.Count == 3);

            foreach (FileData file in files)
            {
                Assert.That(Regex.IsMatch(file.Path, FileManager.RegexRelativePathValidation));
            }

            List<FileData> filesnotRealtive =
                Cloud_Storage_Common.FileManager.GetAllFilesInLocation(
                    TestHelpers.ExampleDataDirectory
                );
            foreach (FileData file in files)
            {
                string absoultepath1 = filesnotRealtive.First(x => x.Name == file.Name).Path;
                string absoulutePath2 = file.getFullPathForBasePath(
                    TestHelpers.ExampleDataDirectory
                );
                Assert.That(absoultepath1 == absoulutePath2);
            }
        }
    }
}
