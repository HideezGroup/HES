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
        Task SendHardwareVaultLicenseStatus(List<HardwareVault> vaults, IList<ApplicationUser> administrators);
        Task SendActivateDataProtectionAsync(IList<ApplicationUser> administrators);
        Task SendUserInvitationAsync(string email, string callbackUrl);
        Task SendEmployeeEnableSsoAsync(string email, string callbackUrl);
        Task SendEmployeeDisableSsoAsync(string email);
        Task SendUserResetPasswordAsync(string email, string callbackUrl);
        Task SendUserConfirmEmailAsync(string userId, string email, string code);
        Task SendSoftwareVaultInvitationAsync(Employee employee, SoftwareVaultActivation activation, DateTime validTo);
        Task SendHardwareVaultActivationCodeAsync(Employee employee, string code);
        Task NotifyWhenPasswordAutoChangedAsync(Employee employee, string accountName);
    }
}