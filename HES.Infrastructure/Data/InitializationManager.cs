using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace HES.Infrastructure.Data
{
    public static class InitializationManager
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }

            return host;
        }

        public static IHost SeedDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                InitializeRoleAndAdmin(scope).Wait();
                InitializeHardwareVaultProfile(scope).Wait();
                InitializeDataProtection(scope).Wait();
            }

            return host;
        }

        private static async Task InitializeRoleAndAdmin(IServiceScope scope)
        {
            using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var roleResult = await roleManager.RoleExistsAsync(ApplicationRoles.Admin);
            if (!roleResult)
            {
                string adminName = "admin@hideez.com";
                string adminPassword = "admin";

                // Create roles
                await roleManager.CreateAsync(new ApplicationRole(ApplicationRoles.Admin));
                await roleManager.CreateAsync(new ApplicationRole(ApplicationRoles.User));
                // Create admin
                var admin = new ApplicationUser { UserName = adminName, Email = adminName, EmailConfirmed = true };
                await userManager.CreateAsync(admin, adminPassword);
                // Add admin to role
                await userManager.AddToRoleAsync(admin, ApplicationRoles.Admin);
            }
        }

        private static async Task InitializeHardwareVaultProfile(IServiceScope scope)
        {
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var profile = await context.HardwareVaultProfiles.FindAsync("default");

            if (profile == null)
            {
                await context.HardwareVaultProfiles.AddAsync(new HardwareVaultProfile
                {
                    Id = "default",
                    Name = "Default",
                    CreatedAt = DateTime.UtcNow,
                    ButtonPairing = true,
                    ButtonConnection = false,
                    ButtonStorageAccess = false,
                    PinPairing = false,
                    PinConnection = false,
                    PinStorageAccess = false,
                    MasterKeyPairing = true,
                    MasterKeyConnection = false,
                    MasterKeyStorageAccess = false,
                    PinExpiration = 86400,
                    PinLength = 4,
                    PinTryCount = 10,
                });

            }
            else
            {
                var profiles = await context.HardwareVaultProfiles.ToListAsync();
                foreach (var item in profiles)
                {
                    item.ButtonPairing = true;
                    item.MasterKeyPairing = true;
                    item.MasterKeyStorageAccess = false;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task InitializeDataProtection(IServiceScope scope)
        {
            var dataProtection = scope.ServiceProvider.GetRequiredService<IDataProtectionService>();
            await dataProtection.InitializeAsync();
        }
    }
}