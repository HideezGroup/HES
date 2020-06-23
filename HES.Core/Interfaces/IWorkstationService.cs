﻿using HES.Core.Entities;
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
    public interface IWorkstationService
    {
        IQueryable<Workstation> WorkstationQuery();
        Task<Workstation> GetWorkstationByIdAsync(string id);
        Task<List<Workstation>> GetWorkstationsAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions);
        Task<int> GetWorkstationsCountAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions);
        Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate);
        Task AddWorkstationAsync(WorkstationInfo workstationInfo);
        Task UpdateWorkstationInfoAsync(WorkstationInfo workstationInfo);
        Task EditWorkstationAsync(Workstation workstation);
        Task ApproveWorkstationAsync(Workstation workstation);
        Task UnapproveWorkstationAsync(string id);
        Task<bool> GetRfidStateAsync(string workstationId);
        Task<bool> CheckIsApprovedAsync(string workstationId);
        Task DetachWorkstationsAsync(List<Workstation> workstations);
        Task UnchangedWorkstationAsync(Workstation workstation);
        IQueryable<WorkstationProximityVault> ProximityVaultQuery();
        Task<List<WorkstationProximityVault>> GetProximityVaultsByWorkstationIdAsync(string workstationId);
        Task<WorkstationProximityVault> GetProximityVaultByIdAsync(string id);
        Task<IList<WorkstationProximityVault>> AddProximityVaultsAsync(string workstationId, string[] vaultsIds);
        Task AddMultipleProximityVaultsAsync(string[] workstationsIds, string[] vaultsIds);
        Task EditProximityVaultAsync(WorkstationProximityVault proximityVault);
        Task DeleteProximityVaultAsync(string proximityVaultId);
        Task DeleteRangeProximityVaultsAsync(List<WorkstationProximityVault> proximityVaults);
        Task DeleteProximityByVaultIdAsync(string vaultId);
        Task<IReadOnlyList<DeviceProximitySettingsDto>> GetProximitySettingsAsync(string workstationId);
    }
}