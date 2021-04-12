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
        private readonly string _baseAddress;

        public EmailSenderService(IHostingEnvironment hostingEnvironment,
                                  IOptions<EmailSettings> emailSettings,
                                  IOptions<ServerSettings> serverSettings,
                                  IHttpContextAccessor httpContextAccessor,
                                  ILogger<EmailSenderService> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _emailSettings = emailSettings;
            _serverSettings = serverSettings;
            _logger = logger;
            _baseAddress = $"{httpContextAccessor?.HttpContext?.Request?.Scheme}://{httpContextAccessor?.HttpContext?.Request?.Host}{httpContextAccessor?.HttpContext?.Request?.PathBase}";

        }

        private async Task SendAsync(string mailTo, string htmlMessage, string subject, params AlternateView[] alternateViews)
        {
            try
            {
                var mailMessage = new MailMessage(_emailSettings.Value.UserName, mailTo);
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, MediaTypeNames.Text.Html);
                htmlView.LinkedResources.Add(CreateImageResource("img_hideez_logo"));
                mailMessage.AlternateViews.Add(htmlView);
                if(alternateViews != null & alternateViews.Length > 0)
                {
                    foreach (var item in alternateViews)
                    {
                        mailMessage.AlternateViews.Add(item);
                    }
                }
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = $"{subject} - {_serverSettings.Value.Name}";

                using var client = new SmtpClient(_emailSettings.Value.Host, _emailSettings.Value.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Value.UserName, _emailSettings.Value.Password),
                    EnableSsl = _emailSettings.Value.EnableSSL
                };

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task SendLicenseChangedAsync(DateTime createdAt, LicenseOrderStatus status, IList<ApplicationUser> administrators)
        {
            var htmlTemplate = await GetTemplateAsync("mail-license-order-status");
            var replacement = new Dictionary<string, string>
                {
                    {"{{createdAt}}", createdAt.ToString() },
                    {"{{status}}", status.ToString() },
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);

            foreach (var admin in administrators)
            {
                await SendAsync(admin.Email, htmlMessage, "Hideez License Order Status Update");
            }
        }

        public async Task SendHardwareVaultLicenseStatus(List<HardwareVault> vaults, IList<ApplicationUser> administrators)
        {
            var htmlMessage = await GetTemplateAsync ("mail-vault-license-status");
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
                await SendAsync(admin.Email, htmlMessage, "Hideez License Status Update");
            }
        }

        public async Task SendActivateDataProtectionAsync(IList<ApplicationUser> administrators)
        {
            var htmlMessage = await GetTemplateAsync("mail-activate-data-protection");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", _serverSettings.Value.Url);

            foreach (var admin in administrators)
            {
                await SendAsync(admin.Email, htmlMessage, "Action required - Hideez Enterprise Server Status Update");
            }
        }

        public async Task SendUserInvitationAsync(string email, string callbackUrl)
        {
            var htmlMessage = await GetTemplateAsync("mail-user-invitation");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", callbackUrl);

            await SendAsync(email, htmlMessage, "Action required - Invitation to Hideez Enterprise Server");
        }

        public async Task SendEmployeeEnableSsoAsync(string email, string callbackUrl)
        {
            var htmlMessage = await GetTemplateAsync ("mail-employee-enable-sso");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", callbackUrl);

            await SendAsync(email, htmlMessage, "Action required - SSO Enabled to Hideez Enterprise Server");
        }

        public async Task SendEmployeeDisableSsoAsync(string email)
        {
            var htmlMessage = await GetTemplateAsync ("mail-employee-disable-sso");

            await SendAsync(email, htmlMessage, "Action required - SSO Disabled to Hideez Enterprise Server");
        }

        public async Task SendUserResetPasswordAsync(string email, string callbackUrl)
        {
            var htmlMessage = await GetTemplateAsync("mail-user-reset-password");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", callbackUrl);

            await SendAsync(email, htmlMessage, "Action required - Password Reset to Hideez Enterprise Server");
        }

        public async Task SendUserConfirmEmailAsync(string userId, string email, string code)
        {
            var callbackUrl = HtmlEncoder.Default.Encode($"{_baseAddress}/confirm-email-change?userId={userId}&code={code}&email={email}");

            var htmlMessage = await GetTemplateAsync("mail-user-confirm-email");
            htmlMessage = htmlMessage.Replace("{{callbackUrl}}", callbackUrl);

            await SendAsync(email, htmlMessage, "Action required - Confirm your email to Hideez Enterprise Server");
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
            if (string.IsNullOrWhiteSpace(employee?.Email))
            {
                return;
            }

            var htmlTemplate = await GetTemplateAsync("mail-hardware-vault-activation-code");
            var replacement = new Dictionary<string, string>
                {
                    {"{{employeeName}}", employee.FullName },
                    {"{{code}}", code },
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);
            await SendAsync(employee.Email, htmlMessage, "Activate Hardware Vault - Hideez Enterprise Server");
        }

        public async Task NotifyWhenPasswordAutoChangedAsync(Employee employee, string accountName)
        {
            if (string.IsNullOrWhiteSpace(employee?.Email))
            {
                return;
            }

            var employeeVaults = string.Join(",", employee.HardwareVaults.Select(x => x.Id).ToList());

            var htmlTemplate = await GetTemplateAsync("mail-password-auto-changed");
            var replacement = new Dictionary<string, string>
                {
                    {"{{employeeName}}", employee.FullName },
                    {"{{employeeVaults}}", employeeVaults },
                    {"{{accountName}}", accountName },
                };

            var htmlMessage = AddDataToTemplate(htmlTemplate, replacement);
            await SendAsync(employee.Email, htmlMessage, "Password Auto Changed - Hideez Enterprise Server ");
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