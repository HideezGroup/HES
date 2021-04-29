using HES.Core.Models.AppSettings;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAppSettingsService
    {
        Task<LicensingSettings> GetLicenseSettingsAsync();
        Task SetLicenseSettingsAsync(LicensingSettings licSettings);
        Task RemoveLicenseSettingsAsync();

        Task<LdapSettings> GetLdapSettingsAsync();
        Task SetLdapSettingsAsync(LdapSettings ldapSettings);
        Task RemoveLdapSettingsAsync();

        Task<AlarmState> GetAlarmStateAsync();
        Task SetAlarmStateAsync(AlarmState alarmState);
    }
}