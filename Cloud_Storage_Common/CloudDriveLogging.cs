using Lombok.NET;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.WithCaller;
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
            return Path.Combine(SharedData.GetAppDirectory(), "logs", "log.log");
        }

        protected CloudDriveLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithCaller()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}\n "
                )
                .WriteTo.File(
                    getLogFilePath(),
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}\n ",
                    shared: true
                )
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
