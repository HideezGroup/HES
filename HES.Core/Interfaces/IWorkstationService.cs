using HES.Core.Entities;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Workstations;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Workstation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService : IDisposable
    {
        IQueryable<Workstation> WorkstationQuery();
        Task<Workstation> GetWorkstationByIdAsync(string id);
        Task<List<Workstation>> GetWorkstationsAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions);
        Task<int> GetWorkstationsCountAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions);
        Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate);
        Task AddWorkstationAsync(WorkstationInfoDto workstationInfoDto);
        Task UpdateWorkstationInfoAsync(WorkstationInfoDto workstationInfoDto);
        Task EditWorkstationAsync(Workstation workstation);
        Task ApproveWorkstationAsync(Workstation workstation);
        Task UnapproveWorkstationAsync(string workstationId);
        Task DeleteWorkstationAsync(string workstationId);
        Task<bool> GetRfidStateAsync(string workstationId);
        Task<bool> CheckIsApprovedAsync(string workstationId);
        Task UnchangedWorkstationAsync(Workstation workstation);
        IQueryable<WorkstationProximityVault> ProximityVaultQuery();
        Task<WorkstationProximityVault> GetProximityVaultByIdAsync(string id);
        Task<List<WorkstationProximityVault>> GetProximityVaultsAsync(DataLoadingOptions<WorkstationDetailsFilter> dataLoadingOptions);
        Task<int> GetProximityVaultsCountAsync(DataLoadingOptions<WorkstationDetailsFilter> dataLoadingOptions);
        Task<WorkstationProximityVault> AddProximityVaultAsync(string workstationId, string vaultId);
        Task DeleteProximityVaultAsync(string proximityVaultId);
        Task DeleteProximityByVaultIdAsync(string vaultId);
        Task<IReadOnlyList<HwVaultProximitySettingsDto>> GetProximitySettingsAsync(string workstationId);
    }
}