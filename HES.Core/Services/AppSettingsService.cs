using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class AppSettingsService : IAppSettingsService
    {
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

        public async Task<T> GetSettingsAsync<T>(string settingsKey)
        {
            if (string.IsNullOrWhiteSpace(settingsKey))
            {
                throw new ArgumentNullException(nameof(settingsKey));
            }

            var appSettings = await _dbContext.AppSettings.FirstOrDefaultAsync(x => x.Id == settingsKey);
            if (appSettings == null)
            {
                return default;
            }

            var value = _dataProtectionService.Decrypt(appSettings.Value);
            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetSettingsAsync<T>(T settings, string settingsKey)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settingsKey))
            {
                throw new ArgumentNullException(nameof(settingsKey));
            }

            var json = JsonSerializer.Serialize(settings);
            var encryptedJson = _dataProtectionService.Encrypt(json);

            var appSettings = await _dbContext.AppSettings.FirstOrDefaultAsync(x => x.Id == settingsKey);
            if (appSettings == null)
            {
                appSettings = new AppSettings
                {
                    Id = settingsKey,
                    Value = encryptedJson
                };

                _dbContext.AppSettings.Add(appSettings);
            }
            else
            {
                appSettings.Value = encryptedJson;
                _dbContext.AppSettings.Update(appSettings);
            }

            await _dbContext.SaveChangesAsync();
        }

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
    }
}