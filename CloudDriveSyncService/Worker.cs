using System.Diagnostics;
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
            _logger.LogInformation("Start wokrker taks");
            try
            {
                //if (!Debugger.IsAttached)
                //{
                //    Debugger.Launch();
                //}
                CloudDriveSyncSystem.Instance.Configuration.LoadConfiguration();
                while (!stoppingToken.IsCancellationRequested) { }

                _logger.LogInformation("Start wokrker Cancleation requesterd - stopin");
                CloudDriveSyncSystem.Instance.FileSyncService.StopAllSync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in the worker.:: {ex.Message}");
            }
            finally
            {
                _logger.LogInformation("Worker stopped.");
            }
        }
    }
}
