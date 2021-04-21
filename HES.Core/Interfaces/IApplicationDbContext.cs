using HES.Core.Entities;
using HES.Core.Models.Audit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; set; }
        DbSet<Employee> Employees { get; set; }
        DbSet<Account> Accounts { get; set; }
        DbSet<HardwareVault> HardwareVaults { get; set; }
        DbSet<HardwareVaultActivation> HardwareVaultActivations { get; set; }
        DbSet<HardwareVaultProfile> HardwareVaultProfiles { get; set; }
        DbSet<HardwareVaultTask> HardwareVaultTasks { get; set; }
        DbSet<HardwareVaultLicense> HardwareVaultLicenses { get; set; }
        DbSet<LicenseOrder> LicenseOrders { get; set; }
        DbSet<SoftwareVault> SoftwareVaults { get; set; }
        DbSet<SoftwareVaultInvitation> SoftwareVaultInvitations { get; set; }
        DbSet<SharedAccount> SharedAccounts { get; set; }
        DbSet<Template> Templates { get; set; }
        DbSet<Workstation> Workstations { get; set; }
        DbSet<WorkstationHardwareVaultPair> WorkstationHardwareVaultPairs { get; set; }
        DbSet<WorkstationEvent> WorkstationEvents { get; set; }
        DbSet<WorkstationSession> WorkstationSessions { get; set; }
        DbSet<Company> Companies { get; set; }
        DbSet<Department> Departments { get; set; }
        DbSet<Position> Positions { get; set; }
        DbSet<DataProtection> DataProtection { get; set; }
        DbSet<AppSettings> AppSettings { get; set; }
        DbSet<Group> Groups { get; set; }
        DbSet<FidoStoredCredential> FidoStoredCredential { get; set; }
        DbSet<GroupMembership> GroupMemberships { get; set; }
        DbSet<SummaryByDayAndEmployee> SummaryByDayAndEmployee { get; set; }
        DbSet<SummaryByEmployees> SummaryByEmployees { get; set; }
        DbSet<SummaryByDepartments> SummaryByDepartments { get; set; }
        DbSet<SummaryByWorkstations> SummaryByWorkstations { get; set; }

        Task SaveChangesAsync();
        void Unchanged<T>(T entity);
        Task<bool> ExistAsync<T>(Expression<Func<T, bool>> predicate) where T : class, new();
    }
}