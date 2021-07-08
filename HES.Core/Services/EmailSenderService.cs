using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Core.Models.SoftwareVault;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IOptions<ServerSettings> _serverSettings;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IHostingEnvironment hostingEnvironment,
                                  IOptions<EmailSettings> emailSettings,
                                  IOptions<ServerSettings> serverSettings,
                                  ILogger<EmailSenderService> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _emailSettings = emailSettings;
            _serverSettings = serverSettings;
            _logger = logger;
        }

        private async Task SendAsync(string mailTo, string htmlMessage, string subject, params AlternateView[] alternateViews)
        {
            try
            {
                var mailMessage = new MailMessage(_emailSettings.Value.UserName, mailTo);
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
                htmlView.LinkedResources.Add(CreateImageResource("mail_logo"));
                mailMessage.AlternateViews.Add(htmlView);
                if (alternateViews != null & alternateViews.Length > 0)
                {
                    foreach (var item in alternateViews)
                    {
                        mailMessage.AlternateViews.Add(item);
                    }
                }
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = subject;

                using var client = new SmtpClient(_emailSettings.Value.Host, _emailSettings.Value.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Value.UserName, _emailSettings.Value.Password),
                    EnableSsl = _emailSettings.Value.EnableSSL
                };

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException == null ? ex.Message : $"{ex.Message} InnerException: {ex.InnerException.Message}");
            }
        }

        public async Task SendLicenseChangedAsync(DateTime createdAt, LicenseOrderStatus status, IList<ApplicationUser> administrators)
        {
            var htmlTemplate = await GetTemplateAsync("mail-license-order-status");

            foreach (var admin in administrators.Where(x => x.EmailConfirmed == true))
            {
                var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, admin.DisplayName) },
                    {"{{body}}", Resources.Resource.Email_LicenseChanged_Body },
                    {"{{description}}", string.Format(Resources.Resource.Email_LicenseChanged_Description, createdAt.ToString(), status.ToString()) },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };

                var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

                await SendAsync(admin.Email, htmlMessage, Resources.Resource.Email_LicenseChanged_Subject);
            }
        }

        public async Task SendHardwareVaultLicenseStatusAsync(List<HardwareVault> vaults, IList<ApplicationUser> administrators)
        {
            var message = new StringBuilder();

            var valid = vaults.Where(d => d.LicenseStatus == VaultLicenseStatus.Valid).OrderBy(d => d.Id).ToList();
            foreach (var item in valid)
            {
                message.Append($"{item.Id} - {item.LicenseStatus}<br/>");
            }

            var warning = vaults.Where(d => d.LicenseStatus == VaultLicenseStatus.Warning).OrderBy(d => d.Id).ToList();
            foreach (var item in warning)
            {
                message.Append($"{item.Id} - {item.LicenseStatus} (90 {Resources.Resource.Email_Common_DaysRemainin})<br/>");
            }

            var critical = vaults.Where(d => d.LicenseStatus == VaultLicenseStatus.Critical).OrderBy(d => d.Id).ToList();
            foreach (var item in critical)
            {
                message.Append($"{item.Id} - {item.LicenseStatus} (30 {Resources.Resource.Email_Common_DaysRemainin})<br/>");
            }

            var expired = vaults.Where(d => d.LicenseStatus == VaultLicenseStatus.Expired).OrderBy(d => d.Id).ToList();
            foreach (var item in expired)
            {
                message.Append($"{item.Id} - {item.LicenseStatus}<br/>");
            }

            var htmlTemplate = await GetTemplateAsync("mail-vault-license-status");

            foreach (var admin in administrators.Where(x => x.EmailConfirmed == true))
            {
                var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, admin.DisplayName) },
                    {"{{body}}", Resources.Resource.Email_HardwareVaultLicenseStatus_Body },
                    {"{{message}}", message.ToString() },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };
                var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

                await SendAsync(admin.Email, htmlMessage, Resources.Resource.Email_HardwareVaultLicenseStatus_Subject);
            }
        }

        public async Task SendActivateDataProtectionAsync(IList<ApplicationUser> administrators)
        {
            var htmlTemplate = await GetTemplateAsync("mail-activate-data-protection");

            foreach (var admin in administrators.Where(x => x.EmailConfirmed = true))
            {
                var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, admin.DisplayName) },
                    {"{{body}}", Resources.Resource.Email_ActivateDataProtection_Body },
                    {"{{btnName}}", Resources.Resource.Email_Common_Btn_Activate },
                    {"{{callbackUrl}}", _serverSettings.Value.Url },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };

                var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

                await SendAsync(admin.Email, htmlMessage, Resources.Resource.Email_ActivateDataProtection_Subject);
            }
        }

        public async Task SendUserInvitationAsync(string email, string callbackUrl)
        {
            var htmlTemplate = await GetTemplateAsync("mail-user-invitation");
            var replacement = new Dictionary<string, string>
            {
                {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, Resources.Resource.Email_Common_Admin) },
                {"{{body}}", Resources.Resource.Email_UserInvitation_Body },
                {"{{linkName}}", Resources.Resource.Email_Common_Link },
                {"{{callbackUrl}}", callbackUrl },
                {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
            };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            await SendAsync(email, htmlMessage, Resources.Resource.Email_UserInvitation_Subject);
        }

        public async Task SendEmployeeEnableSsoAsync(Employee employee, string callbackUrl)
        {
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                _logger.LogWarning($"Trying to send an email with an empty email field, employee - {employee.FullName}");
                return;
            }

            var htmlTemplate = await GetTemplateAsync("mail-employee-enable-sso");
            var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, employee.FullName) },
                    {"{{body}}", Resources.Resource.Email_EmployeeEnableSso_Body },
                    {"{{linkName}}", Resources.Resource.Email_Common_Link },
                    {"{{callbackUrl}}", callbackUrl },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            await SendAsync(employee.Email, htmlMessage, Resources.Resource.Email_EmployeeEnableSso_Subject);
        }

        public async Task SendEmployeeDisableSsoAsync(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                _logger.LogWarning($"Trying to send an email with an empty email field, employee - {employee.FullName}");
                return;
            }

            var htmlTemplate = await GetTemplateAsync("mail-employee-disable-sso");
            var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, employee.FullName) },
                    {"{{body}}", Resources.Resource.Email_EmployeeDisableSso_Body },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            await SendAsync(employee.Email, htmlMessage, Resources.Resource.Email_EmployeeDisableSso_Subject);
        }

        public async Task SendUserResetPasswordAsync(string email, string callbackUrl)
        {
            var htmlTemplate = await GetTemplateAsync("mail-user-reset-password");
            var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, Resources.Resource.Email_Common_Admin) },
                    {"{{body}}", Resources.Resource.Email_UserResetPassword_Body },
                    {"{{btnName}}", Resources.Resource.Email_Common_Btn_ResetPassword },
                    {"{{callbackUrl}}", callbackUrl },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            await SendAsync(email, htmlMessage, Resources.Resource.Email_UserResetPassword_Subject);
        }

        public async Task SendUserConfirmEmailAsync(ApplicationUser user, string newEmail, string callbackUrl)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning($"Trying to send an email with an empty email field, user - {user.DisplayName}");
                return;
            }

            var htmlTemplate = await GetTemplateAsync("mail-user-confirm-email");
            var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, user.DisplayName) },
                    {"{{body}}", Resources.Resource.Email_UserConfirmEmail_Body },
                    {"{{btnName}}", Resources.Resource.Email_Common_Btn_Confirm },
                    {"{{callbackUrl}}", callbackUrl },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            await SendAsync(newEmail, htmlMessage, Resources.Resource.Email_UserConfirmEmail_Subject);
        }

        public async Task SendSoftwareVaultInvitationAsync(Employee employee, SoftwareVaultActivation activation, DateTime validTo)
        {
            if (string.IsNullOrWhiteSpace(employee?.Email))
            {
                return;
            }

            var htmlTemplate = await GetTemplateAsync("mail-software-vault-invitation");
            var replacement = new Dictionary<string, string>
                {
                    {"{{employeeName}}", employee.FirstName },
                    {"{{validTo}}", validTo.Date.ToShortDateString() },
                    {"{{serverAddress}}", activation.ServerAddress },
                    {"{{activationId}}", activation.ActivationId },
                    {"{{activationCode}}", activation.ActivationCode.ToString() },
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            var htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);

            var code = $"{activation.ServerAddress}\n{activation.ActivationId}\n{activation.ActivationCode}";
            var qrCode = GetQRCode(code);

            var img = new LinkedResource(qrCode)
            {
                ContentId = "QRCode",
                TransferEncoding = TransferEncoding.Base64
            };
            img.ContentType.MediaType = MediaTypeNames.Image.Jpeg;
            img.ContentType.Name = img.ContentId;
            img.ContentLink = new Uri("cid:" + img.ContentId);

            htmlView.LinkedResources.Add(img);

            await SendAsync(employee.Email, htmlMessage, "Hideez Software Vault application", htmlView);
        }

        public async Task SendHardwareVaultActivationCodeAsync(Employee employee, string code)
        {
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                _logger.LogWarning($"Trying to send an email with an empty email field, employee - {employee.FullName}");
                return;
            }

            var htmlTemplate = await GetTemplateAsync("mail-hardware-vault-activation-code");
            var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, employee.FullName) },
                    {"{{body}}", Resources.Resource.Email_HardwareVaultActivationCode_Body },
                    {"{{code}}", code },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            await SendAsync(employee.Email, htmlMessage, Resources.Resource.Email_HardwareVaultActivationCode_Subject);
        }

        public async Task SendNotifyWhenPasswordAutoChangedAsync(Employee employee, string accountName)
        {
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                _logger.LogWarning($"Trying to send an email with an empty email field, employee - {employee.FullName}");
                return;
            }

            var employeeVaults = string.Join(",", employee.HardwareVaults.Select(x => x.Id).ToList());

            var htmlTemplate = await GetTemplateAsync("mail-password-auto-changed");
            var replacement = new Dictionary<string, string>
                {
                    {"{{dear}}", string.Format(Resources.Resource.Email_Common_Dear, employee.FullName) },
                    {"{{body}}", string.Format(Resources.Resource.Email_NotifyWhenPasswordAutoChanged_Body, accountName, employeeVaults) },
                    {"{{description}}", Resources.Resource.Email_NotifyWhenPasswordAutoChanged_Description },
                    {"{{yourServer}}", Resources.Resource.Email_Common_YourServer }
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            await SendAsync(employee.Email, htmlMessage, Resources.Resource.Email_NotifyWhenPasswordAutoChanged_Subject);
        }

        private async Task<string> GetTemplateAsync(string name)
        {
            var path = Path.Combine(_hostingEnvironment.WebRootPath, "templates", $"{name}.html");
            using var reader = File.OpenText(path);
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static string AddDataToTemplate(string htmlTemplate, Dictionary<string, string> replacements)
        {
            foreach (var item in replacements)
            {
                htmlTemplate = htmlTemplate.Replace(item.Key, item.Value);
            }

            return htmlTemplate;
        }

        private LinkedResource CreateImageResource(string name)
        {
            var img = new LinkedResource(Path.Combine(_hostingEnvironment.WebRootPath, "templates", $"{name}.png"))
            {
                ContentId = name,
                TransferEncoding = TransferEncoding.Base64

            };
            img.ContentType.MediaType = MediaTypeNames.Image.Jpeg;
            img.ContentType.Name = img.ContentId;
            img.ContentLink = new Uri("cid:" + img.ContentId);
            return img;
        }

        private static MemoryStream GetQRCode(string text)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            var qrCodeImageData = BitmapToBytes(qrCodeImage);
            return new MemoryStream(qrCodeImageData);
        }

        private static byte[] BitmapToBytes(Bitmap img)
        {
            using var stream = new MemoryStream();
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}