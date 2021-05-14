using Fido2NetLib;
using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.HostedServices;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Core.Models.Saml;
using HES.Core.Services;
using HES.Infrastructure;
using HES.Web.Components;
using HES.Web.Providers;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Util;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }
        public bool Saml2Enabled { get; }
        public bool ReverseProxyHandleSSL { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            #region Environment variables

            var server = configuration["MYSQL_SRV"];
            var port = configuration["MYSQL_PORT"];
            var db = configuration["MYSQL_DB"];
            var uid = configuration["MYSQL_UID"];
            var pwd = configuration["MYSQL_PWD"];
            if (server != null && port != null && db != null && uid != null && pwd != null)
            {
                configuration["ConnectionStrings:DefaultConnection"] = $"server={server};port={port};database={db};uid={uid};pwd={pwd}";
            }

            var email_host = configuration["EMAIL_HOST"];
            var email_port = configuration["EMAIL_PORT"];
            var email_ssl = configuration["EMAIL_SSL"];
            var email_user = configuration["EMAIL_USER"];
            var email_pwd = configuration["EMAIL_PWD"];
            if (email_host != null && email_port != null && email_ssl != null && email_user != null && email_pwd != null)
            {
                configuration["EmailSender:Host"] = email_host;
                configuration["EmailSender:Port"] = email_port;
                configuration["EmailSender:EnableSSL"] = email_ssl;
                configuration["EmailSender:UserName"] = email_user;
                configuration["EmailSender:Password"] = email_pwd;
            }

            var srv_name = configuration["SRV_NAME"];
            var srv_url = configuration["SRV_URL"];
            if (srv_name != null && srv_url != null)
            {
                configuration["ServerSettings:Name"] = srv_name;
                configuration["ServerSettings:Url"] = srv_url;
            }

            var dataprotectoin_pwd = configuration["DATAPROTECTION_PWD"];
            if (dataprotectoin_pwd != null)
            {
                configuration["DataProtection:Password"] = dataprotectoin_pwd;
            }

            #endregion

            #region Reverse Proxy

            ReverseProxyHandleSSL = configuration.GetValue<bool>("ServerSettings:ReverseProxyHandleSSL");

            #endregion

            #region SAML

            if (!string.IsNullOrWhiteSpace(configuration.GetValue<string>("Saml2:SigningCertificatePassword")))
            {
                Saml2Enabled = true;
            }

            #endregion

            Configuration = configuration;
            Env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Database

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection")),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)));

            #endregion

            #region Services

            services.AddScoped<IApplicationDbContext>(x => x.GetService<ApplicationDbContext>());
            services.AddScoped(typeof(IDataTableService<,>), typeof(DataTableService<,>));
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IHardwareVaultService, HardwareVaultService>();
            services.AddScoped<IHardwareVaultTaskService, HardwareVaultTaskService>();
            services.AddScoped<IWorkstationService, WorkstationService>();
            services.AddScoped<IWorkstationAuditService, WorkstationAuditService>();
            services.AddScoped<ISharedAccountService, SharedAccountService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<IApplicationUserService, ApplicationUserService>();
            services.AddScoped<IOrgStructureService, OrgStructureService>();
            services.AddScoped<IRemoteWorkstationConnectionsService, RemoteWorkstationConnectionsService>();
            services.AddScoped<IRemoteDeviceConnectionsService, RemoteDeviceConnectionsService>();
            services.AddScoped<IRemoteTaskService, RemoteTaskService>();
            services.AddScoped<IEmailSenderService, EmailSenderService>();
            services.AddScoped<ILicenseService, LicenseService>();
            services.AddScoped<IAppSettingsService, AppSettingsService>();
            services.AddScoped<IToastService, ToastService>();
            services.AddScoped<IModalDialogService, ModalDialogService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<ILdapService, LdapService>();
            services.AddScoped<ISoftwareVaultService, SoftwareVaultService>();
            services.AddScoped<IBreadcrumbsService, BreadcrumbsService>();
            services.AddScoped<IFido2Service, Fido2Service>();
            services.AddScoped<IIdentityApiClient, IdentityApiClient>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IDataProtectionService, DataProtectionService>();
            services.AddSingleton<IPageSyncService, PageSyncService>();

            services.AddHostedService<RemoveLogsHostedService>();
            services.AddHostedService<LicenseHostedService>();
            services.AddHostedService<ActiveDirectoryHostedService>();

            services.AddHttpClient();
            services.AddHttpClient("HES").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });
            services.AddHttpClient("Splunk").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });
            services.AddSignalR();
            services.AddMemoryCache();

            #endregion

            #region Configuration

            services.Configure<Fido2Configuration>(Configuration.GetSection("Fido2"));
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSender"));
            services.Configure<ServerSettings>(Configuration.GetSection("ServerSettings"));

            #endregion

            #region Identity

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<RegisterSecurityKeyTokenProvider<ApplicationUser>>(RegisterSecurityKeyTokenConstants.TokenName);

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;
            });

            #endregion

            #region Cookie

            services.ConfigureApplicationCookie(config =>
            {
                config.LoginPath = new PathString(Routes.Login);
                config.LogoutPath = new PathString(Routes.Logout);
                config.AccessDeniedPath = new PathString(Routes.AccessDenied);

                config.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            #endregion

            #region SAML

            services.Configure<Saml2Configuration>(Configuration.GetSection("Saml2"));
            services.Configure<Saml2RelyingParties>(Configuration.GetSection("Saml2Sp"));

            if (Saml2Enabled)
            {
                services.Configure<Saml2Configuration>(saml2Configuration =>
                {
                    saml2Configuration.SigningCertificate = CertificateUtil.Load(Env.MapToPhysicalFilePath(Configuration["Saml2:SigningCertificateFile"]), Configuration["Saml2:SigningCertificatePassword"]);
                    saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);
                });
                services.AddSaml2();
            }

            #endregion

            #region Localization

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo(CultureConstants.EN),
                    new CultureInfo(CultureConstants.UA)
                };
                options.DefaultRequestCulture = new RequestCulture(CultureConstants.EN);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            #endregion

            services.AddMvc().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddControllers();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Register the Swagger generator
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HES API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                if (!ReverseProxyHandleSSL)
                {
                    app.UseHsts();
                }
            }

            app.UseRequestLocalization();

            app.UseStatusCodePages();
            app.UseStaticFiles();

            app.UseRouting();

            if (!ReverseProxyHandleSSL)
            {
                app.UseHttpsRedirection();
            }

            if (Saml2Enabled)
            {
                app.UseSaml2();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HES API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<DeviceHub>("/deviceHub");
                endpoints.MapHub<AppHub>("/appHub");
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger<Startup>>();
            logger.LogInformation($"Server started {ServerConstants.Version}");
        }
    }
}