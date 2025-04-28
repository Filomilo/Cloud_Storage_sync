using Cloud_Storage_Common;
using Cloud_Storage_Desktop_lib;

namespace CloudDriveSyncService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;

        public Worker()
        {
            _logger = CloudDriveLogging.Instance.GetLogger("Service");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                CloudDriveSyncSystem.Instance.Configuration.LoadConfiguration();
                while (!stoppingToken.IsCancellationRequested) { }
                CloudDriveSyncSystem.Instance.FileSyncService.StopAllSync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the worker.");
            }
            finally
            {
                _logger.LogInformation("Worker stopped.");
            }
        }
    }
}
