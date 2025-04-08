using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Tests;
using Cloud_Storage_Server.Services;
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

    public TestConfig() { }

    public TestConfig(string storageLocation)
    {
        this.StorageLocation = storageLocation;
    }
}

public class TestCredentialMangager : ICredentialManager
{
    private string _token = "";

    public void SaveToken(string token)
    {
        _token = token;
    }

    public string GetToken()
    {
        return _token;
    }

    public void RemoveToken()
    {
        _token = "";
    }

    public string GetDeviceID()
    {
        return JwtHelpers.GetDeviceIDFromToken(_token);
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

        public static string GetNewTmpDir(string fodlerName)
        {
            return TmpDirecotry + fodlerName + "\\";
        }

        public static IServerConnection getTestServerConnetion()
        {
            HttpClient _testServer = new MyWebApplication().CreateDefaultClient();
            return new ServerConnection(_testServer, new TestCredentialMangager());
        }

        public static IConfiguration GetTestConfig()
        {
            return new TestConfig();
        }

        public static void UploudAccontDataToLoggedUser(
            IServerConnection serverConnection,
            IConfiguration Configuration,
            IFileRepositoryService fileRepositoryService
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
                UploadAction uploadAction = new UploadAction(
                    serverConnection,
                    Configuration,
                    fileRepositoryService,
                    file
                );
                Assert.DoesNotThrow(() =>
                {
                    uploadAction.ActionToRun.Invoke();
                });
            }
        }

        public static void RemoveTmpDirectory()
        {
            if (Directory.Exists(TmpDirecotry))
                Directory.Delete(TmpDirecotry, true);
        }

        public static string CreateTmpFile(string dir, string fileContent)
        {
            string fileName = Path.GetFileName(Path.GetTempFileName());
            FileStream newlyCreatedFile = File.Create(dir + fileName);
            newlyCreatedFile.Write(Encoding.ASCII.GetBytes(fileContent));
            newlyCreatedFile.Close();
            return fileName;
        }

        private const long Timeout = 200000;

        public static void EnsureTrue(Func<bool> func)
        {
            bool state = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                state = func();
                if (state == true)
                    break;
                Thread.Sleep(100);
                if (stopwatch.ElapsedMilliseconds > Timeout)
                    throw new TimeoutException($"Ensure true timouet {Timeout}");
            }
        }

        public static FileSystemService GetDeafultFileSystemService()
        {
            return new FileSystemService("dataStorage\\");
        }
    }
}
