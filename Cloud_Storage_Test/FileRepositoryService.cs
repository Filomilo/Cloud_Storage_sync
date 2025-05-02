using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib.Services;
using NUnit.Framework;

namespace Cloud_Storage_Test
{
    [TestFixture]
    class FileRepositoryServiceTest
    {
        [Test]
        public void savedAndGetFile()
        {
            FileRepositoryService fileRepositoryService = new FileRepositoryService(
                new TestDbContextGenerator1()
            );
            fileRepositoryService.Reset();
            UploudFileData uploudFileData = new UploudFileData()
            {
                Extenstion = ".txt",
                Name = "test",
                Path = "test",
                Hash = "33",
            };
            fileRepositoryService.AddNewFile(new LocalFileData(uploudFileData));
            Assert.That(
                fileRepositoryService.GetAllFiles().Count() == 1,
                $"file not saved proprly expected one file got {fileRepositoryService.GetAllFiles().Count()}"
            );
        }
    }
}
