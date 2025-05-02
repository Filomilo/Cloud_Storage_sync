using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Interfaces;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Desktop_lib;
using Cloud_Storage_Desktop_lib.Actions;
using Cloud_Storage_Desktop_lib.Interfaces;
using Cloud_Storage_Desktop_lib.Services;
using Cloud_Storage_Desktop_lib.Tests;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using NUnit.Framework;
using AbstractDataBaseContext = Cloud_Storage_Desktop_lib.Interfaces.AbstractDataBaseContext;

public class TestHadnlerChecker : AbstactHandler
{
    public bool didRun = false;

    public override object Handle(object request)
    {
        didRun = true;
        return null;
    }
}

public class TestDataBaseSerwerContext : Cloud_Storage_Server.Database.AbstractDataBaseContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("serwer");
    }
}

public class TestDataBaseSerwerContextGenerator : IDataBaseContextGenerator
{
    public Cloud_Storage_Server.Database.AbstractDataBaseContext GetDbContext()
    {
        return new TestDataBaseSerwerContext();
    }
}

public class TestConfig : IConfiguration
{
    public string TmpDirecotry =
        AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "tmp\\";

    public string ApiUrl
    {
        get { return ""; }
        set { }
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

    public void LoadConfiguration() { }

    public void SaveConfiguration() { }

    public event ConfigurationChange? OnConfigurationChange;

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

    public string GetEmail()
    {
        return JwtHelpers.GetEmailFromToken(_token);
    }
}

public class TestWebScoket : IWebSocketWrapper
{
    private ILogger logger = CloudDriveLogging.Instance.GetLogger("TestWebScoket");
    private WebSocketClient _webSocketClient;
    private WebSocket _webSocket = new ClientWebSocket();
    private Dictionary<string, string> _headers = new Dictionary<string, string>();

    public TestWebScoket(WebSocketClient webSocketClient)
    {
        _webSocketClient = webSocketClient;
    }

    public void Connect(Uri url, CancellationToken cancellationToken)
    {
        _webSocketClient.ConfigureRequest = (request) =>
        {
            foreach (var keyValuePair in _headers)
            {
                request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
            }
        };
        _webSocket = _webSocketClient.ConnectAsync(url, cancellationToken).Result;
    }

    public WebSocketReceiveResult ReceiveAsync(
        ArraySegment<byte> buffer,
        CancellationToken cancellationToken
    )
    {
        return this._webSocket.ReceiveAsync(buffer, cancellationToken).Result;
    }

    public WebSocketState State
    {
        get { return this._webSocket.State; }
    }

    public void Close(
        WebSocketCloseStatus closeStatus,
        string? statusDescription,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (
                this._webSocket.State != WebSocketState.Aborted
                | this._webSocket.State != WebSocketState.Closed
            )
                this._webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken)
                    .Dispose();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex.Message);
        }
    }

    public void SetRequestHeader(string str1, string value)
    {
        if (this._headers.ContainsKey(str1))
            _headers.Remove(str1);
        _headers.Add(str1, value);
    }
}

public class DataBAseContext1 : AbstractDataBaseContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Files1");
    }
}

public class DataBAseContext2 : AbstractDataBaseContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Files2");
    }
}

public class TestDbContextGenerator1 : IDbContextGenerator
{
    public AbstractDataBaseContext GetDbContext()
    {
        return new DataBAseContext1();
    }
}

public class TestDbContextGenerator2 : IDbContextGenerator
{
    public AbstractDataBaseContext GetDbContext()
    {
        return new DataBAseContext2();
    }
}

namespace Cloud_Storage_Test
{
    class TestHelpers
    {
        public static IServerConfig serverConfig = new ServerConfig();

        public static string ExampleDataDirectory =
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
            + "testData\\";

        /// <summary>
        /// Clean after usage
        /// </summary>
        public static string TmpDirecotry =
            AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
            + "tmp\\";

        public static string getDummyLocation(int i)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))
                + $"dummy{i}\\";
        }

        public static string GetNewTmpDir(string fodlerName, bool shoudlCreate = false)
        {
            String path = TmpDirecotry + fodlerName + "\\";
            if (shoudlCreate)
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static IServerConnection getTestServerConnetion()
        {
            HttpClient _testServer = new MyWebApplication().CreateDefaultClient();
            return new ServerConnection(
                _testServer,
                new TestCredentialMangager(),
                new WebSocketWrapper()
            );
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
            TestHelpers.EnsureTrue(() =>
            {
                try
                {
                    if (Directory.Exists(TmpDirecotry))
                        Directory.Delete(TmpDirecotry, true);
                }
                catch (Exception ex)
                {
                    return false;
                }

                return true;
            });
        }

        public static string CreateTmpFile(string dir, string fileContent, int i)
        {
            string fileName =
                Path.GetFileName(Path.GetFileNameWithoutExtension(Path.GetTempFileName()))
                + $"_{i}.tmp";
            FileStream newlyCreatedFile = File.Create(dir + fileName);
            newlyCreatedFile.Write(Encoding.ASCII.GetBytes(fileContent));
            newlyCreatedFile.Close();
            return fileName;
        }

        private const long Timeout = 2000;

        public static void EnsureTrue(Func<bool> func, long timeout = Timeout)
        {
            if (Debugger.IsAttached)
            {
                timeout *= 100;
            }

            bool state = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                state = func();
                if (state == true)
                    break;
                Thread.Sleep(100);
                if (stopwatch.ElapsedMilliseconds > timeout)
                    throw new TimeoutException($"Ensure true timouet {timeout}");
            }
        }

        public static void EnsureNotThrows(Action action, long timeout = Timeout)
        {
            if (Debugger.IsAttached)
            {
                timeout *= 100;
            }

            bool state = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Thread.Sleep(100);
                    if (stopwatch.ElapsedMilliseconds > timeout)
                        throw ex;
                    else
                    {
                        continue;
                    }
                }
                break;
            }
        }

        public static FileSystemService GetDeafultFileSystemService()
        {
            return new FileSystemService(serverConfig);
        }

        public static void ResetDatabase()
        {
            using (var context = new DataBAseContext1())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            using (var context = new DataBAseContext2())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            using (var context = new SqliteDataBaseContextGenerator().GetDbContext())
            {
                Assert.DoesNotThrow(
                    () =>
                    {
                        if (context.Database.CanConnect())
                        {
                            EnsureTrue(() =>
                            {
                                try
                                {
                                    context.Database.EnsureDeleted();
                                    return true;
                                }
                                catch (Exception ex)
                                {
                                    return false;
                                }
                            });
                        }
                    },
                    "Cannot connect databse"
                );

                context.Database.EnsureCreated();
            }
        }

        public static string GetSeverStoragePath()
        {
            return Directory.GetCurrentDirectory() + "\\dataStorage\\";
        }

        public static void ClearServerStorage()
        {
            string serverStoragePath = GetSeverStoragePath();
            if (Directory.Exists(serverStoragePath))
                Directory.Delete(serverStoragePath, true);
        }

        internal static string GetPassoword()
        {
            return Guid.NewGuid().ToString("N");
        }

        internal static string getEmail()
        {
            return $"test{Guid.NewGuid().ToString().Split("-")[0]}@wp.pl";
        }
    }
}
