using System.Diagnostics;
using Cloud_Storage_Common;
using CloudDriveSyncService;

try
{
    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddHostedService<Worker>();
        })
        .UseWindowsService(options =>
        {
            options.ServiceName = "CloudDriveSync";
        })
        .Build();

    var logger = CloudDriveLogging.Instance.GetLogger("Service");
    EventLog.WriteEntry(
        "CloudDriveSync",
        $"Service logging file: {CloudDriveLogging.Instance.getLogFilePath()}",
        EventLogEntryType.Information
    );
    for (int i = 0; i < 100000; i++)
    {
        Thread.Sleep(100);
        logger.LogInformation($"test : {i}\n");
    }
    logger.LogInformation("Starting host...");

    await host.RunAsync();
}
catch (Exception ex)
{
    EventLog.WriteEntry("CloudDriveSync", $"Service crashed: {ex}", EventLogEntryType.Error);
    throw;
}
