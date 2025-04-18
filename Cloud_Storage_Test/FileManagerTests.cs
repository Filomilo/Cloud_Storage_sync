using System.Text.RegularExpressions;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Test;
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

        [Test]
        public void GetFilePathParamsFormRelativePathTest()
        {
            string path = "test/test.txt";
            FileManager.GetFilePathParamsFormRelativePath(
                path,
                out string directory,
                out string name,
                out string extesnion
            );
            Assert.That(
                directory == "test",
                $"Expeccted directory to be [[test]] but instead is [[{directory}]]"
            );
            Assert.That(
                name == "test",
                $"Expeccted directory to be [[test]] but instead is [[{name}]]"
            );
            Assert.That(
                extesnion == ".txt",
                $"Expeccted extenison to be [[.txt]] but instead is [[{extesnion}]]"
            );
        }
    }
}
