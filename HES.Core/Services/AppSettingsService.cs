using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private static readonly ConcurrentDictionary<string, object> _cache = new();
        private readonly IApplicationDbContext _dbContext;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AppSettingsService> _logger;

        public AppSettingsService(IApplicationDbContext applicationDbContext, IDataProtectionService dataProtectionService, IMemoryCache memoryCache, ILogger<AppSettingsService> logger)
        {
            _dbContext = applicationDbContext;
            _dataProtectionService = dataProtectionService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        #region License Settings

        public async Task<LicensingSettings> GetLicenseSettingsAsync()
        {
            var settings = await _dbContext.AppSettings.FirstOrDefaultAsync(x => x.Id == ServerConstants.Licensing);
            if (settings == null)
            {
                return null;
            }

            var deserialized = JsonSerializer.Deserialize<LicensingSettings>(settings.Value);
            deserialized.ApiKey = _dataProtectionService.Decrypt(deserialized.ApiKey);

            return deserialized;
        }

        public async Task SetLicenseSettingsAsync(LicensingSettings licSettings)
        {
            if (licSettings == null)
            {
                throw new ArgumentNullException(nameof(licSettings));
            }

            licSettings.ApiKey = _dataProtectionService.Encrypt(licSettings.ApiKey);

            var json = JsonSerializer.Serialize(licSettings);

            var appSettings = await _dbContext.AppSettings.FindAsync(ServerConstants.Licensing);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Licensing,
                    Value = json
                };
                _dbContext.AppSettings.Add(appSettings);
            }
            else
            {
                appSettings.Value = json;
                _dbContext.AppSettings.Update(appSettings);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveLicenseSettingsAsync()
        {
            var appSettings = await _dbContext.AppSettings.FindAsync(ServerConstants.Licensing);
            if (appSettings != null)
            {
                _dbContext.AppSettings.Remove(appSettings);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region Ldap Settings

        public async Task<LdapSettings> GetLdapSettingsAsync()
        {
            var settings = await _dbContext.AppSettings.FirstOrDefaultAsync(x => x.Id == ServerConstants.Ldap);
            if (settings == null)
            {
                return null;
            }

            var deserialized = JsonSerializer.Deserialize<LdapSettings>(settings.Value);
            deserialized.Password = _dataProtectionService.Decrypt(deserialized.Password);

            return deserialized;
        }

        public async Task SetLdapSettingsAsync(LdapSettings ldapSettings)
        {
            if (ldapSettings == null)
            {
                throw new ArgumentNullException(nameof(ldapSettings));
            }

            ldapSettings.Password = _dataProtectionService.Encrypt(ldapSettings.Password);

            var json = JsonSerializer.Serialize(ldapSettings);

            var appSettings = await _dbContext.AppSettings.FindAsync(ServerConstants.Ldap);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Ldap,
                    Value = json
                };
                _dbContext.AppSettings.Add(appSettings);
            }
            else
            {
                appSettings.Value = json;
                _dbContext.AppSettings.Update(appSettings);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveLdapSettingsAsync()
        {
            var appSettings = await _dbContext.AppSettings.FindAsync(ServerConstants.Ldap);
            if (appSettings != null)
            {
                _dbContext.AppSettings.Remove(appSettings);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region Alarm Settings

        public async Task<AlarmState> GetAlarmStateAsync()
        {
            var alarmState = _memoryCache.Get<AlarmState>(ServerConstants.Alarm);

            if (alarmState != null)
            {
                return alarmState;
            }

            alarmState = new AlarmState();

            var _path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "alarmstate.json");

            if (!File.Exists(_path))
            {
                _memoryCache.Set(ServerConstants.Alarm, alarmState);
                return alarmState;
            }

            string json = string.Empty;
            try
            {
                using (var reader = new StreamReader(_path))
                {
                    json = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    return alarmState;
                }

                alarmState = JsonSerializer.Deserialize<AlarmState>(json);
                _memoryCache.Set(ServerConstants.Alarm, alarmState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return alarmState;
        }

        public async Task SetAlarmStateAsync(AlarmState alarmState)
        {
            _memoryCache.Set(ServerConstants.Alarm, alarmState);

            try
            {
                var json = JsonSerializer.Serialize(alarmState);
                var _path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "alarmstate.json");
                using (var writer = new StreamWriter(_path, false))
                {
                    await writer.WriteLineAsync(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        #endregion

        #region Splunk Settrings

        public async Task<SplunkSettings> GetSplunkSettingsAsync()
        {
            if (_cache.TryGetValue(ServerConstants.Splunk, out object value))
            {
                return (SplunkSettings)value;
            }

            var settings = await _dbContext.AppSettings.FirstOrDefaultAsync(x => x.Id == ServerConstants.Splunk);
            if (settings == null)
            {
                _cache.AddOrUpdate(ServerConstants.Splunk, settings, (key, existing) => settings);
                return null;
            }

            var deserialized = JsonSerializer.Deserialize<SplunkSettings>(settings.Value);
            deserialized.Token = _dataProtectionService.Decrypt(deserialized.Token);
            _cache.AddOrUpdate(ServerConstants.Splunk, deserialized, (key, existing) => deserialized);
            return deserialized;
        }

        public async Task SetSplunkSettingsAsync(SplunkSettings splunkSettings)
        {
            if (splunkSettings == null)
            {
                throw new ArgumentNullException(nameof(splunkSettings));
            }

            splunkSettings.Token = _dataProtectionService.Encrypt(splunkSettings.Token);

            var json = JsonSerializer.Serialize(splunkSettings);

            var appSettings = await _dbContext.AppSettings.FindAsync(ServerConstants.Splunk);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Splunk,
                    Value = json
                };
                _dbContext.AppSettings.Add(appSettings);
            }
            else
            {
                appSettings.Value = json;
                _dbContext.AppSettings.Update(appSettings);
            }

            await _dbContext.SaveChangesAsync();
            _cache.TryRemove(ServerConstants.Splunk, out object _);
        }

        public async Task RemoveSplunkSettingsAsync()
        {
            var appSettings = await _dbContext.AppSettings.FindAsync(ServerConstants.Splunk);
            if (appSettings != null)
            {
                _dbContext.AppSettings.Remove(appSettings);
                await _dbContext.SaveChangesAsync();
            }
            _cache.TryRemove(ServerConstants.Splunk, out object _);
        }

        #endregion
    }
}