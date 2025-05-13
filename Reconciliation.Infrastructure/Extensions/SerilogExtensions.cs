using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Infrastructure.Extensions
{
    public static class SerilogExtensions
    {
        public static IHostBuilder UseCustomSerilog(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((context, services, configuration) =>
            {
                configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    );

            });
        }
    }
}
