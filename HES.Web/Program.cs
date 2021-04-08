using HES.Infrastructure.Data;
using HES.Web.Initialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System.Threading.Tasks;

namespace HES.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await ApplicationDbContextSeed.MigrateDatabaseAsync(host);
            await ApplicationDbContextSeed.SeedDatabaseAsync(host);
            await AppInitializer.ExecuteAsync(host);

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.UseStartup<Startup>();
             })
            .ConfigureLogging(logging =>
             {
                 logging.ClearProviders();
                 logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
             })
            .UseNLog();  // NLog: Setup NLog for Dependency injection
    }
}