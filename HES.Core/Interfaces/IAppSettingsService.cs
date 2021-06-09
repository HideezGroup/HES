using HES.Core.Entities;
using HES.Core.Models.AppSettings;
using System.Collections.Generic;
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

        Task<SplunkSettings> GetSplunkSettingsAsync();
        Task SetSplunkSettingsAsync(SplunkSettings splunkSettings);
        Task RemoveSplunkSettingsAsync();

        Task<List<SamlRelyingParty>> GetSaml2RelyingPartiesAsync();
        Task<SamlRelyingParty> GetSaml2RelyingPartyAsync(string relyingPartyId);
        Task AddSaml2RelyingPartyAsync(SamlRelyingParty relyingParty);
        Task EditSaml2RelyingPartyAsync(SamlRelyingParty relyingParty);
        void UnchangedSaml2RelyingParty(SamlRelyingParty relyingParty);
        Task RemoveSaml2RelyingPartyAsync(string relyingPartyId);
    }
}