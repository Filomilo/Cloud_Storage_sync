using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lombok.NET;
using Microsoft.Extensions.Logging;

namespace Cloud_Storage_Common
{
    [Singleton]
    public partial class CloudDriveLogging
    {
        public ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }
}
