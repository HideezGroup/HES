using HES.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace HES.Web.Initialization
{
    public static class AppInitializer
    {
        public static async Task ExecuteAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            await InitializeDataProtectionAsync(scope);
            await InitializeAlarmStateAsync(scope);
        }

        private static async Task InitializeDataProtectionAsync(IServiceScope scope)
        {
            var dataProtection = scope.ServiceProvider.GetRequiredService<IDataProtectionService>();
            await dataProtection.InitializeAsync();
        }

        private static async Task InitializeAlarmStateAsync(IServiceScope scope)
        {
            var appSettingsService = scope.ServiceProvider.GetService<IAppSettingsService>();
            await appSettingsService.GetAlarmStateAsync();
        }
    }
}