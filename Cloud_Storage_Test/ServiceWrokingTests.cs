using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Database;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_desktop.Logic;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Cloud_Storage_Test
{
    [TestFixture]
    public class ServiceWrokingTests
    {
        private ServiceOperator serviceOperator;
        private Configuration configuration;
        private string tmpSyncdirectory;
        private AbstractDataBaseContext localDataBase;

        [SetUp]
        public void setup()
        {
            tmpSyncdirectory = TestHelpers.GetNewTmpDir(
                "TestSync_" + Guid.NewGuid().ToString().Split("-")[0],
                true
            );

            configuration = new Configuration();
            configuration.StorageLocation = tmpSyncdirectory;
            configuration.ApiUrl = ServerControlHelpers.Instance.GetIpConnection();
            configuration.MaxStimulationsFileSync = 5;
            configuration.SaveConfiguration();

            localDataBase = new LocalSqlLiteDbGeneraor().GetDbContext();
            Assert.That(
                localDataBase.Files.ToArray().Length == 0,
                $"Local database not empty: {localDataBase.Files.Count()}"
            );

            serviceOperator = new ServiceOperator();
            if (serviceOperator.Exist)
            {
                serviceOperator.DeleteService();
            }
            serviceOperator.CreateService();
            Assert.That(serviceOperator.Exist);
            Assert.That(serviceOperator.IsServiceRunning() == false);

            ServerControlHelpers.Instance.StartServer();
        }

        [TearDown]
        public void teardown()
        {
            TestHelpers.RemoveTmpDirectory();
            ServerControlHelpers.Instance.StopServer();
            serviceOperator.DeleteService();
            localDataBase.Files.ExecuteDelete();
            LogFileController.ClearlogFile();
        }

        [Test]
        public void startServiceWithoutServerConneciton()
        {
            ServerControlHelpers.Instance.StopServer();
            Assert.DoesNotThrow(() =>
            {
                serviceOperator.StartService();
            });
        }

        [Test]
        public void EnsureThatocaldataBaseWorksWithServerDisconneted()
        {
            ServerControlHelpers.Instance.StopServer();
            serviceOperator.StartService();
            Thread.Sleep(1000);
            TestHelpers.CreateTmpFile(this.tmpSyncdirectory, "TestData", 1);
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return this.localDataBase.Files.Count() == 1;
                    });
                },
                $"Local databse di not update \n\n {LogFileController.GetLogFileContent()} "
            );
        }
    }
}
