using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.SoftwareVault;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class EmailSenderService : IEmailSenderService, IDisposable
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IApplicationUserService applicationUserService,
                                  IHostingEnvironment env,
                                  IConfiguration config,
                                  ILogger<EmailSenderService> logger)
        {
            _applicationUserService = applicationUserService;
            _env = env;
            _config = config;
            _logger = logger;
        }

        private async Task SendAsync(MailMessage mailMessage, EmailSettings settings)
        {
            try
            {
                using var client = new SmtpClient(settings.Host, settings.Port)
                {
                    Credentials = new NetworkCredential(settings.UserName, settings.Password),
                    EnableSsl = settings.EnableSSL
                };

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task SendLicenseChangedAsync(DateTime createdAt, LicenseOrderStatus status)
        {
            EmailSettings emailSettings = await GetEmailSettingsAsync();
            ServerSettings serverSettings = await GetServerSettingsAsync();

            var administrators = await _applicationUserService.GetAllAdministratorsAsync();

            var htmlMessage = GetTemplate("mail-license-order-status");

            htmlMessage = htmlMessage.Replace("{{createdAt}}", createdAt.ToString()).Replace("{{status}}", status.ToString());

            foreach (var admin in administrators)
            {
                MailMessage mailMessage = new MailMessage(emailSettings.UserName, admin.Email);
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
                htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
                mailMessage.AlternateViews.Add(htmlView);
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = $"Hideez License Order Status Update - {serverSettings.Name}";

                await SendAsync(mailMessage, emailSettings);
            }
        }

        public async Task SendHardwareVaultLicenseStatus(List<HardwareVault> vaults)
        {
            EmailSettings emailSettings = await GetEmailSettingsAsync();
            ServerSettings serverSettings = await GetServerSettingsAsync();

            var administrators = await _applicationUserService.GetAllAdministratorsAsync();

            var htmlMessage = GetTemplate("mail-vault-license-status");

            var message = new StringBuilder();

            var valid = vaults.Where(d => d.LicenseStatus == VaultLicenseStatus.Valid).OrderBy(d => d.Id).ToList();
            foreach (var item in valid)
            {
                message.Append($"{item.Id} - {item.LicenseStatus}<br/>");
            }

            var warning = vaults.Where(d => d.LicenseStatus == VaultLicenseStatus.Warning).OrderBy(d => d.Id).ToList();
            foreach (var item in warning)
            {
                message.Append($"{item.Id} - {item.LicenseStatus} (90 days remainin)<br/>");
            }

            var critical = vaults.Where(d => d.LicenseStatus == VaultLicenseStatus.Critical).OrderBy(d => d.Id).ToList();
            foreach (var item in critical)
            {
                message.Append($"{item.Id} - {item.LicenseStatus} (30 days remainin)<br/>");
            }

            var expired = vaults.Where(d => d.LicenseStatus == VaultLicenseStatus.Expired).OrderBy(d => d.Id).ToList();
            foreach (var item in expired)
            {
                message.Append($"{item.Id} - {item.LicenseStatus}<br/>");
            }

            htmlMessage = htmlMessage.Replace("{{message}}", message.ToString());

            foreach (var admin in administrators)
            {
                MailMessage mailMessage = new MailMessage(emailSettings.UserName, admin.Email);
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
                htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
                mailMessage.AlternateViews.Add(htmlView);
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = $"Hideez License Status Update - {serverSettings.Name}";

                await SendAsync(mailMessage, emailSettings);
            }
        }

        public async Task SendActivateDataProtectionAsync()
        {
            EmailSettings emailSettings = await GetEmailSettingsAsync();
            ServerSettings serverSettings = await GetServerSettingsAsync();

            var administrators = await _applicationUserService.GetAllAdministratorsAsync();

            var htmlMessage = GetTemplate("mail-activate-data-protection");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", $"{serverSettings.Url}");

            foreach (var admin in administrators)
            {
                MailMessage mailMessage = new MailMessage(emailSettings.UserName, admin.Email);
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
                htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
                mailMessage.AlternateViews.Add(htmlView);
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = $"Action required - Hideez Enterprise Server Status Update - {serverSettings.Name}";

                await SendAsync(mailMessage, emailSettings);
            }
        }

        public async Task SendUserInvitationAsync(string email, string callbackUrl)
        {
            var emailSettings = await GetEmailSettingsAsync();
            var serverSettings = await GetServerSettingsAsync();

            var htmlMessage = GetTemplate("mail-user-invitation");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", callbackUrl);

            MailMessage mailMessage = new MailMessage(emailSettings.UserName, email);
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
            htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = $"Action required - Invitation to Hideez Enterprise Server - {serverSettings.Name}";

            await SendAsync(mailMessage, emailSettings);
        }

        public async Task SendUserResetPasswordAsync(string email, string callbackUrl)
        {
            var emailSettings = await GetEmailSettingsAsync();
            var serverSettings = await GetServerSettingsAsync();

            var htmlMessage = GetTemplate("mail-user-reset-password");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", callbackUrl);

            MailMessage mailMessage = new MailMessage(emailSettings.UserName, email);
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
            htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = $"Action required - Password Reset to Hideez Enterprise Server - {serverSettings.Name}";

            await SendAsync(mailMessage, emailSettings);
        }

        public async Task SendUserConfirmEmailAsync(string email, string callbackUrl)
        {
            var emailSettings = await GetEmailSettingsAsync();
            var serverSettings = await GetServerSettingsAsync();

            var htmlMessage = GetTemplate("mail-user-confirm-email");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", callbackUrl);

            MailMessage mailMessage = new MailMessage(emailSettings.UserName, email);
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
            htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = $"Action required - Confirm your email to Hideez Enterprise Server - {serverSettings.Name}";

            await SendAsync(mailMessage, emailSettings);
        }

        public async Task SendSoftwareVaultInvitationAsync(Employee employee, SoftwareVaultActivation activation, DateTime validTo)
        {
            if (employee.Email == null)
                throw new ArgumentNullException(nameof(employee.Email));

            var emailSettings = await GetEmailSettingsAsync();

            var htmlMessage = GetTemplate("mail-software-vault-invitation");
            htmlMessage = htmlMessage.Replace("{{employeeName}}", employee.FirstName)
                .Replace("{{validTo}}", validTo.Date.ToShortDateString())
                .Replace("{{serverAddress}}", activation.ServerAddress)
                .Replace("{{activationId}}", activation.ActivationId)
                .Replace("{{activationCode}}", activation.ActivationCode.ToString());

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                                                         htmlMessage,
                                                         Encoding.UTF8,
                                                         MediaTypeNames.Text.Html);

            var code = $"{activation.ServerAddress}\n{activation.ActivationId}\n{activation.ActivationCode}";
            var qrCode = GetQRCode(code);

            string mediaType = MediaTypeNames.Image.Jpeg;
            LinkedResource img = new LinkedResource(qrCode, mediaType);
            img.ContentId = "QRCode";
            img.ContentType.MediaType = mediaType;
            img.TransferEncoding = TransferEncoding.Base64;
            img.ContentType.Name = img.ContentId;
            img.ContentLink = new Uri("cid:" + img.ContentId);

            htmlView.LinkedResources.Add(img);

            MailMessage mailMessage = new MailMessage(emailSettings.UserName, employee.Email);
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "Hideez Software Vault application";

            await SendAsync(mailMessage, emailSettings);
        }

        public async Task SendHardwareVaultActivationCodeAsync(Employee employee, string code)
        {
            var emailSettings = await GetEmailSettingsAsync();
            var serverSettings = await GetServerSettingsAsync();

            var htmlMessage = GetTemplate("mail-hardware-vault-activation-code");
            htmlMessage = htmlMessage.Replace("{{employee}}", employee.FullName).Replace("{{code}}", code);

            MailMessage mailMessage = new MailMessage(emailSettings.UserName, employee.Email);
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
            htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = $"Activate Hardware Vault - Hideez Enterprise Server - {serverSettings.Name}";

            await SendAsync(mailMessage, emailSettings);
        }

        public async Task NotifyWhenPasswordAutoChangedAsync(Employee employee, string accountName)
        {
            if (string.IsNullOrWhiteSpace(employee?.Email))
                return;

            var emailSettings = await GetEmailSettingsAsync();
            var serverSettings = await GetServerSettingsAsync();

            var employeeVaults = string.Join(",", employee.HardwareVaults.Select(x => x.Id).ToList());
            var htmlMessage = GetTemplate("mail-password-auto-changed");
            htmlMessage = htmlMessage.Replace("{{employeeName}}", employee.FullName)
                .Replace("{{employeeVaults}}", employeeVaults)
                .Replace("{{accountName}}", accountName);

            MailMessage mailMessage = new MailMessage(emailSettings.UserName, employee.Email);
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
            htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
            mailMessage.AlternateViews.Add(htmlView);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = $"Password Auto Changed - Hideez Enterprise Server - {serverSettings.Name}";

            await SendAsync(mailMessage, emailSettings);
        }

        private Task<EmailSettings> GetEmailSettingsAsync()
        {
            var settings = new EmailSettings()
            {
                Host = _config.GetValue<string>("EmailSender:Host"),
                Port = _config.GetValue<int>("EmailSender:Port"),
                EnableSSL = _config.GetValue<bool>("EmailSender:EnableSSL"),
                UserName = _config.GetValue<string>("EmailSender:UserName"),
                Password = _config.GetValue<string>("EmailSender:Password")
            };

            return Task.FromResult(settings);
        }

        private Task<ServerSettings> GetServerSettingsAsync()
        {
            var settings = new ServerSettings()
            {
                Name = _config.GetValue<string>("ServerSettings:Name"),
                Url = _config.GetValue<string>("ServerSettings:Url")
            };

            return Task.FromResult(settings);
        }

        private string GetTemplate(string name)
        {
            var path = Path.Combine(_env.WebRootPath, "templates", $"{name}.html");
            using StreamReader reader = File.OpenText(path);
            return reader.ReadToEnd();
        }

        private LinkedResource CreateImageResource(string name)
        {
            string mediaType = MediaTypeNames.Image.Jpeg;
            var img = new LinkedResource(Path.Combine(_env.WebRootPath, "templates", $"{name}.png"));
            img.ContentId = name;
            img.ContentType.MediaType = mediaType;
            img.TransferEncoding = TransferEncoding.Base64;
            img.ContentType.Name = img.ContentId;
            img.ContentLink = new Uri("cid:" + img.ContentId);
            return img;
        }

        private MemoryStream GetQRCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            var qrCodeImageData = BitmapToBytes(qrCodeImage);
            return new MemoryStream(qrCodeImageData);
        }

        private byte[] BitmapToBytes(Bitmap img)
        {
            using MemoryStream stream = new MemoryStream();
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        public void Dispose()
        {
            _applicationUserService.Dispose();
        }
    }
}