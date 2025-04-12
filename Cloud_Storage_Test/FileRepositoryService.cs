using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            UploudFileData uploudFileData = new UploudFileData()
            {
                Extenstion = ".txt",
                Name = "test",
                Path = "test",
                Hash = "33",
            };
            fileRepositoryService.AddNewFile((LocalFileData)uploudFileData);
            Assert.That(fileRepositoryService.GetAllFiles().Count() == 1, "file not saved proprly");
        }
    }
}
