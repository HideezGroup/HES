using HES.Core.Constants;
using HES.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace HES.Infrastructure.Data
{
    public static class ApplicationDbContextSeed
    {
        public static async Task MigrateDatabaseAsync(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    await context.Database.MigrateAsync();
                }
            }
        }

        public static async Task SeedDatabaseAsync(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                await InitializeRoleAndAdmin(scope);
                await InitializeHardwareVaultProfile(scope);
            }
        }

        private static async Task InitializeRoleAndAdmin(IServiceScope scope)
        {
            using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var roleResult = await roleManager.RoleExistsAsync(ApplicationRoles.Admin);
            if (!roleResult)
            {
                string adminEmail = "admin@hideez.com";
                string adminPassword = "admin";            

                // Create roles
                await roleManager.CreateAsync(new ApplicationRole(ApplicationRoles.Admin));
                await roleManager.CreateAsync(new ApplicationRole(ApplicationRoles.User));
                // Create admin
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "admin",
                    LastName = "hideez"
                };
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
    }
}