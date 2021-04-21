using HES.Core.Entities;
using HES.Core.Models.AppSettings;
using HES.Core.Models.SoftwareVault;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISoftwareVaultService
    {
        //IQueryable<SoftwareVault> SoftwareVaultQuery();
        //Task<List<SoftwareVault>> GetSoftwareVaultsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, SoftwareVaultFilter filter);
        //Task<int> GetVaultsCountAsync(string searchText, SoftwareVaultFilter filter);
        //IQueryable<SoftwareVaultInvitation> SoftwareVaultInvitationQuery();
        //Task<List<SoftwareVaultInvitation>> GetSoftwareVaultInvitationsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, SoftwareVaultInvitationFilter filter);
        //Task<int> GetInvitationsCountAsync(string searchText, SoftwareVaultInvitationFilter filter);
        //Task CreateAndSendInvitationAsync(Employee employee, ServerSettings server, DateTime validTo);
        //Task ResendInvitationAsync(Employee employee, ServerSettings server, string invitationId);
        //Task<SoftwareVaultInvitation> DeleteInvitationAsync(string invitationId);
    }
}