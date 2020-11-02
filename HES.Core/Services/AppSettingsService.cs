using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class AppSettingsService : IAppSettingsService, IDisposable
    {
        private readonly IAsyncRepository<AppSettings> _appSettingsRepository;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AppSettingsService> _logger;

        public AppSettingsService(IAsyncRepository<AppSettings> appSettingsRepository, IDataProtectionService dataProtectionService, IMemoryCache memoryCache, ILogger<AppSettingsService> logger)
        {
            _appSettingsRepository = appSettingsRepository;
            _dataProtectionService = dataProtectionService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<LicensingSettings> GetLicensingSettingsAsync()
        {
            var licensing = await _appSettingsRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ServerConstants.Licensing);

            if (licensing == null)
                return null;

            var deserialized = JsonConvert.DeserializeObject<LicensingSettings>(licensing.Value);
            deserialized.ApiKey = _dataProtectionService.Decrypt(deserialized.ApiKey);

            return deserialized;
        }

        public async Task SetLicensingSettingsAsync(LicensingSettings licensing)
        {
            if (licensing == null)
                throw new ArgumentNullException(nameof(LicensingSettings));

            licensing.ApiKey = _dataProtectionService.Encrypt(licensing.ApiKey);

            var json = JsonConvert.SerializeObject(licensing);

            var appSettings = await _appSettingsRepository.GetByIdAsync(ServerConstants.Licensing);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Licensing,
                    Value = json
                };
                await _appSettingsRepository.AddAsync(appSettings);
            }
            else
            {
                appSettings.Value = json;
                await _appSettingsRepository.UpdateAsync(appSettings);
            }
        }

        public async Task<EmailSettings> GetEmailSettingsAsync()
        {
            var settings = await _appSettingsRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ServerConstants.Email);

            if (settings == null)
                return null;

            var deserialized = JsonConvert.DeserializeObject<EmailSettings>(settings.Value);
            deserialized.Password = _dataProtectionService.Decrypt(deserialized.Password);

            return deserialized;
        }

        public async Task SetEmailSettingsAsync(EmailSettings email)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            email.Password = _dataProtectionService.Encrypt(email.Password);

            var json = JsonConvert.SerializeObject(email);

            var appSettings = await _appSettingsRepository.GetByIdAsync(ServerConstants.Email);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Email,
                    Value = json
                };
                await _appSettingsRepository.AddAsync(appSettings);
            }
            else
            {
                appSettings.Value = json;
                await _appSettingsRepository.UpdateAsync(appSettings);
            }
        }

        public async Task<AlarmState> GetAlarmStateAsync()
        {
            var alarmState = _memoryCache.Get<AlarmState>(ServerConstants.Alarm);

            if (alarmState != null)
                return alarmState;

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
                    return alarmState;

                alarmState = JsonConvert.DeserializeObject<AlarmState>(json);
                _memoryCache.Set(ServerConstants.Alarm, alarmState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return alarmState;
        }

        public async Task<bool> GetAlarmEnabledAsync()
        {
            var alarmState = await GetAlarmStateAsync();
            return alarmState != null ? alarmState.IsAlarm : false;
        }

        public async Task SetAlarmStateAsync(AlarmState alarmState)
        {
            _memoryCache.Set(ServerConstants.Alarm, alarmState);

            try
            {
                var json = JsonConvert.SerializeObject(alarmState);
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

        public async Task<ServerSettings> GetServerSettingsAsync()
        {
            var server = await _appSettingsRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ServerConstants.Server);

            if (server == null)
                return null;

            return JsonConvert.DeserializeObject<ServerSettings>(server.Value);
        }

        public async Task SetServerSettingsAsync(ServerSettings server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(ServerSettings));

            var json = JsonConvert.SerializeObject(server);

            var appSettings = await _appSettingsRepository.GetByIdAsync(ServerConstants.Server);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Server,
                    Value = json
                };
                await _appSettingsRepository.AddAsync(appSettings);
            }
            else
            {
                appSettings.Value = json;
                await _appSettingsRepository.UpdateAsync(appSettings);
            }
        }

        public async Task<LdapSettings> GetLdapSettingsAsync()
        {
            var domain = await _appSettingsRepository.Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ServerConstants.Domain);

            if (domain == null)
                return null;

            var deserialized = JsonConvert.DeserializeObject<LdapSettings>(domain.Value);
            deserialized.Password = _dataProtectionService.Decrypt(deserialized.Password);

            return deserialized;
        }

        public async Task SetLdapSettingsAsync(LdapSettings ldapSettings)
        {
            if (ldapSettings == null)
                throw new ArgumentNullException(nameof(LdapSettings));

            ldapSettings.Password = _dataProtectionService.Encrypt(ldapSettings.Password);

            var json = JsonConvert.SerializeObject(ldapSettings);

            var appSettings = await _appSettingsRepository.GetByIdAsync(ServerConstants.Domain);

            if (appSettings == null)
            {
                appSettings = new AppSettings()
                {
                    Id = ServerConstants.Domain,
                    Value = json
                };
                await _appSettingsRepository.AddAsync(appSettings);
            }
            else
            {
                appSettings.Value = json;
                await _appSettingsRepository.UpdateAsync(appSettings);
            }
        }

        public void Dispose()
        {
            _appSettingsRepository.Dispose();
        }
    }
}