using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Database;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_desktop.Logic;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private IDbContextGenerator localDataBasectxGenreator;
        private IDataBaseContextGenerator ServerContextGenerator;
        private Microsoft.Extensions.Logging.ILogger logger = CloudDriveLogging.Instance.GetLogger(
            "ServiceWrokingTests"
        );

        [SetUp]
        public void setup()
        {
            ServerContextGenerator = new SqliteDataBaseContextGenerator();
            ServerControlHelpers.Instance.StartServer();
            tmpSyncdirectory = TestHelpers.GetNewTmpDir(
                "TestSync_" + Guid.NewGuid().ToString().Split("-")[0],
                true
            );

            configuration = new Configuration();
            configuration.StorageLocation = tmpSyncdirectory;
            configuration.ApiUrl = ServerControlHelpers.Instance.GetIpConnection();
            configuration.MaxStimulationsFileSync = 5;
            configuration.SaveConfiguration();

            ServerConnection serverConnection = new ServerConnection(
                ServerControlHelpers.Instance.GetIpConnection(),
                new CredentialManager(),
                new NullWebSocket()
            );
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(() =>
                    {
                        return serverConnection.CheckIfHelathy();
                    });
                },
                "Server not healthy"
            );
            serverConnection.Register(TestHelpers.getEmail(), TestHelpers.GetPassoword());

            localDataBasectxGenreator = new LocalSqlLiteDbGeneraor();
            using (var ctx = localDataBasectxGenreator.GetDbContext())
            {
                ctx.Files.ExecuteDelete();
                Assert.That(
                    ctx.Files.ToArray().Length == 0,
                    $"Local database not empty: {ctx.Files.Count()}"
                );
            }

            serviceOperator = new ServiceOperator();
            if (serviceOperator.Exist)
            {
                serviceOperator.DeleteService();
            }
            serviceOperator.CreateService();
            Assert.That(serviceOperator.Exist);
            Assert.That(serviceOperator.IsServiceRunning() == false);

            Assert.That(() =>
            {
                using (var ctx = ServerContextGenerator.GetDbContext())
                {
                    return !ctx.Files.Any();
                }
            });
        }

        [TearDown]
        public void teardown()
        {
            logger.LogInformation("Teard down");

            ServerControlHelpers.Instance.StopServer();
            logger.LogDebug("Delete serivce");
            serviceOperator.DeleteService();
            using (var ctx = localDataBasectxGenreator.GetDbContext())
            {
                ctx.Files.ExecuteDelete();
                ctx.SaveChanges();
            }
            logger.LogDebug("Clear log");
            LogFileController.ClearlogFile();
            logger.LogDebug("Remove tmp dir");
            TestHelpers.RemoveTmpDirectory();
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
                        bool res = false;
                        using (var ctx = localDataBasectxGenreator.GetDbContext())
                        {
                            res = ctx.Files.Count() == 1;
                        }

                        return res;
                    });
                },
                $"Local databse di not update \n\n {LogFileController.GetLogFileContent()} "
            );
        }

        [Test]
        public void TheSameFilesAfterChannignSyncFoldrToEMptyOne()
        {
            LogFileController.ClearlogFile();
            serviceOperator.StartService();
            string filename = TestHelpers.CreateTmpFile(this.tmpSyncdirectory, "TestData", 1);
            Thread.Sleep(10000);
            int amountOFFile = 0;
            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(
                        () =>
                        {
                            using (var ctx = ServerContextGenerator.GetDbContext())
                            {
                                amountOFFile = ctx.Files.Count();
                                return amountOFFile == 1;
                            }
                        },
                        10000
                    );
                },
                $"Server doesnt  contain onr file in db but [[{amountOFFile}]]:: \n  [[\n {LogFileController.GetLogFileContent()}\n]]"
            );

            string newSyncPath = TestHelpers.GetNewTmpDir("NewSyncPAth", true);
            configuration.StorageLocation = newSyncPath;
            configuration.SaveConfiguration();

            serviceOperator.StopService();
            logger.LogInformation(
                "-----------------------------------------------------------------Server for file start"
            );
            LogFileController.ClearlogFile();
            serviceOperator.StartService();

            Assert.DoesNotThrow(
                () =>
                {
                    TestHelpers.EnsureTrue(
                        () =>
                        {
                            return FileManager.GetAllFilesInLocation(newSyncPath).Count == 1;
                        },
                        10000
                    );
                },
                $"New locaiotn does not have old file \n\n [[\n {LogFileController.GetLogFileContent()}\n]] "
            );
        }
    }
}
