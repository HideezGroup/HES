using HES.Core.Entities;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService
    {
        Task<Workstation> GetWorkstationByIdAsync(string workstationId);
        Task<List<Workstation>> GetWorkstationsAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions);
        Task<int> GetWorkstationsNotApproveCountAsync();
        Task<int> GetWorkstationsCountAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions);
        Task<int> GetWorkstationsCountAsync();
        Task AddWorkstationAsync(WorkstationInfoDto workstationInfoDto);
        Task EditWorkstationInfoAsync(WorkstationInfoDto workstationInfoDto);
        Task EditWorkstationAsync(Workstation workstation);
        Task ApproveWorkstationAsync(Workstation workstation);
        Task UnapproveWorkstationAsync(string workstationId);
        Task DeleteWorkstationAsync(string workstationId);
        Task<bool> CheckIsRFIDEnabledAsync(string workstationId);
        Task<bool> CheckIsApprovedAsync(string workstationId);
        void UnchangedWorkstation(Workstation workstation);
        Task<WorkstationHardwareVaultPair> GetWorkstationHardwareVaultPairByIdAsync(string pairId);
        Task<List<WorkstationHardwareVaultPair>> GetWorkstationHardwareVaultPairsAsync(DataLoadingOptions<WorkstationDetailsFilter> dataLoadingOptions);
        Task<int> GetWorkstationHardwareVaultPairsCountAsync(DataLoadingOptions<WorkstationDetailsFilter> dataLoadingOptions);
        Task<WorkstationHardwareVaultPair> CreateWorkstationHardwareVaultPairAsync(string workstationId, string vaultId);
        Task DeleteWorkstationHardwareVaultPairAsync(string pairId);
        Task DeleteWorkstationHardwareVaultPairsByVaultIdAsync(string vaultId);
        Task<IReadOnlyList<HwVaultProximitySettingsDto>> GetWorkstationHardwareVaultPairSettingsAsync(string workstationId);
    }
}