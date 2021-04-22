using HES.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HES.Core.HostedServices
{
    public class ActiveDirectoryHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ActiveDirectoryHostedService> _logger;

        private const int _synchronizationUsersIntervalInHours = 1;
        private const int _changePasswordIntervalInHours = 12;

        private Timer _synchronizationUsersTimer;
        private Timer _changePassworTimer;

        public ActiveDirectoryHostedService(IServiceProvider services, ILogger<ActiveDirectoryHostedService> logger)
        {
            _logger = logger;
            _serviceProvider = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _synchronizationUsersTimer = new Timer(async (object state) => await SyncUsersAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(_synchronizationUsersIntervalInHours));
            _changePassworTimer = new Timer(async (object state) => await ChangePasswordAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(_changePasswordIntervalInHours));
            return Task.CompletedTask;
        }

        private async Task SyncUsersAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var appSettingsService = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();

                var ldapSettings = await appSettingsService.GetLdapSettingsAsync();
                if (ldapSettings == null)
                    return;

                var ldapService = scope.ServiceProvider.GetRequiredService<ILdapService>();
                await ldapService.SyncUsersAsync(ldapSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task ChangePasswordAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var appSettingsService = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();

                var ldapSettings = await appSettingsService.GetLdapSettingsAsync();
                if (ldapSettings == null)                
                    return;                

                var ldapService = scope.ServiceProvider.GetRequiredService<ILdapService>();
                await ldapService.ChangePasswordWhenExpiredAsync(ldapSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _synchronizationUsersTimer?.Change(Timeout.Infinite, 0);
            _changePassworTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _synchronizationUsersTimer?.Dispose();
            _changePassworTimer?.Dispose();
        }
    }
}