using Lombok.NET;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cloud_Storage_Common
{
    [Singleton]
    public partial class CloudDriveLogging
    {
        private ILogger TestLogger = null;

        public ILogger GetLogger(string name)
        {
            if (TestLogger != null)
            {
                return TestLogger;
            }

            return loggerFactory.CreateLogger(name);
        }

        public string getLogFilePath()
        {
            return SharedData.GetAppDirectory() + "\\logs\\log.log";
        }

        protected CloudDriveLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(getLogFilePath())
                .CreateLogger();
        }

        private ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog();
        });

        public void SetTestLogger(ILogger logger)
        {
            this.TestLogger = logger;
            ;
        }
    }
}
