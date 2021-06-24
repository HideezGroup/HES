using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.SoftwareVault;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendLicenseChangedAsync(DateTime createdAt, LicenseOrderStatus status, IList<ApplicationUser> administrators);
        Task SendHardwareVaultLicenseStatusAsync(List<HardwareVault> vaults, IList<ApplicationUser> administrators);
        Task SendActivateDataProtectionAsync(IList<ApplicationUser> administrators);
        Task SendUserInvitationAsync(string email, string callbackUrl);
        Task SendEmployeeEnableSsoAsync(Employee employee, string callbackUrl);
        Task SendEmployeeDisableSsoAsync(Employee employee);
        Task SendUserResetPasswordAsync(string email, string callbackUrl);
        Task SendUserConfirmEmailAsync(ApplicationUser user, string email, string code);
        Task SendSoftwareVaultInvitationAsync(Employee employee, SoftwareVaultActivation activation, DateTime validTo);
        Task SendHardwareVaultActivationCodeAsync(Employee employee, string code);
        Task SendNotifyWhenPasswordAutoChangedAsync(Employee employee, string accountName);
    }
}