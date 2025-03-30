using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Tests;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NUnit.Framework;

public class TestConfig : IConfiguration
{
    public static string TmpDirecotry =
        AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "tmp\\";

    public string ApiUrl
    {
        get { return ""; }
    }

    public int MaxStimulationsFileSync
    {
        get { return 5; }
    }

    public string StorageLocation
    {
        get { return TmpDirecotry; }
        set { TmpDirecotry = value; }
    }
}

namespace Cloud_Storage_Test
{
    class TestHelpers
    {
        public static string ExampleDataDirectory =
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
            + "testData\\";

        /// <summary>
        /// Clean after usage
        /// </summary>
        public static string TmpDirecotry =
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
            + "tmp\\";

        public static IServerConnection getTestServerConnetion()
        {
            HttpClient _testServer = new MyWebApplication().CreateDefaultClient();
            return new ServerConnection(_testServer);
        }

        public static IConfiguration GetTestConfig()
        {
            return new TestConfig();
        }

        public static void UploudAccontDataToLoggedUser(
            IServerConnection serverConnection,
            IConfiguration Configuration
        )
        {
            Configuration.StorageLocation = TestHelpers.ExampleDataDirectory;
            List<FileData> files = FileManager.GetAllFilesInLocation(
                TestHelpers.ExampleDataDirectory
            );
            foreach (FileData fileData in files)
            {
                UploudFileData file = FileManager.GetUploadFileData(
                    fileData.getFullFilePathForBasePath(Configuration.StorageLocation),
                    Configuration.StorageLocation
                );
                UploadAction uploadAction = new UploadAction(serverConnection, Configuration, file);
                Assert.DoesNotThrow(() =>
                {
                    uploadAction.ActionToRun.Invoke();
                });
            }
        }
    }
}
