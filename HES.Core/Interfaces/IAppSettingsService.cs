using HES.Core.Models.AppSettings;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAppSettingsService
    {
        Task<T> GetSettingsAsync<T>(string settingsKey);
        Task SetSettingsAsync<T>(T settings, string settingsKey);
        Task<AlarmState> GetAlarmStateAsync();
        Task SetAlarmStateAsync(AlarmState alarmState);
    }
}