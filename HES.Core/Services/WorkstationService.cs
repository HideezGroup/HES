using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Workstations;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationService : IWorkstationService
    {
        private readonly IApplicationDbContext _dbContext;

        public WorkstationService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Workstation

        public async Task<Workstation> GetWorkstationByIdAsync(string workstationId)
        {
            return await _dbContext.Workstations
                .Include(x => x.Department.Company)
                .FirstOrDefaultAsync(x => x.Id == workstationId);
        }

        public async Task<List<Workstation>> GetWorkstationsAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions)
        {
            return await WorkstationQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetWorkstationsNotApproveCountAsync()
        {
            return await _dbContext.Workstations.Where(x => !x.Approved).CountAsync();
        }

        public async Task<int> GetWorkstationsCountAsync(DataLoadingOptions<WorkstationFilter> dataLoadingOptions)
        {
            return await WorkstationQuery(dataLoadingOptions).CountAsync();
        }

        public async Task<int> GetWorkstationsCountAsync()
        {
            return await _dbContext.Workstations
                .Include(x => x.Department.Company)
                .Include(x => x.WorkstationProximityVaults)
                .CountAsync();
        }

        private IQueryable<Workstation> WorkstationQuery(DataLoadingOptions<WorkstationFilter> dataLoadingOptions)
        {
            var query = _dbContext.Workstations
                .Include(x => x.Department.Company)
                .Include(x => x.WorkstationProximityVaults)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Name != null)
                {
                    query = query.Where(w => w.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Domain != null)
                {
                    query = query.Where(w => w.Domain.Contains(dataLoadingOptions.Filter.Domain, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.ClientVersion != null)
                {
                    query = query.Where(w => w.ClientVersion.Contains(dataLoadingOptions.Filter.ClientVersion, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.OS != null)
                {
                    query = query.Where(w => w.OS.Contains(dataLoadingOptions.Filter.OS, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.IP != null)
                {
                    query = query.Where(w => w.IP.Contains(dataLoadingOptions.Filter.IP, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.LastSeenStartDate != null)
                {
                    query = query.Where(w => w.LastSeen >= dataLoadingOptions.Filter.LastSeenStartDate);
                }
                if (dataLoadingOptions.Filter.LastSeenEndDate != null)
                {
                    query = query.Where(x => x.LastSeen <= dataLoadingOptions.Filter.LastSeenEndDate);
                }
                if (dataLoadingOptions.Filter.RFID != null)
                {
                    query = query.Where(x => x.RFID == dataLoadingOptions.Filter.RFID);
                }
                if (dataLoadingOptions.Filter.Approved != null)
                {
                    query = query.Where(x => x.Approved == dataLoadingOptions.Filter.Approved);
                }
                if (dataLoadingOptions.Filter.Online != null)
                {
                    var workstationIds = RemoteWorkstationConnectionsService.GetWorkstationsOnlineIds();

                    if (dataLoadingOptions.Filter.Online.Value == true)
                    {
                        query = query.Where(x => workstationIds.Contains(x.Id));
                    }
                    else
                    {
                        query = query.Where(x => !workstationIds.Contains(x.Id));
                    }
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Domain.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.ClientVersion.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.OS.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.IP.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(Workstation.Name):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case nameof(Workstation.Domain):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Domain) : query.OrderByDescending(x => x.Domain);
                    break;
                case nameof(Workstation.ClientVersion):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.ClientVersion) : query.OrderByDescending(x => x.ClientVersion);
                    break;
                case nameof(Workstation.Department.Company):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Company.Name) : query.OrderByDescending(x => x.Department.Company.Name);
                    break;
                case nameof(Workstation.Department):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Name) : query.OrderByDescending(x => x.Department.Name);
                    break;
                case nameof(Workstation.OS):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.OS) : query.OrderByDescending(x => x.OS);
                    break;
                case nameof(Workstation.IP):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.IP) : query.OrderByDescending(x => x.IP);
                    break;
                case nameof(Workstation.LastSeen):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LastSeen) : query.OrderByDescending(x => x.LastSeen);
                    break;
                case nameof(Workstation.RFID):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.RFID) : query.OrderByDescending(x => x.RFID);
                    break;
                case nameof(Workstation.Approved):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Approved) : query.OrderByDescending(x => x.Approved);
                    break;
            }

            return query;
        }

        public async Task AddWorkstationAsync(WorkstationInfoDto workstationInfoModel)
        {
            if (workstationInfoModel == null)
            {
                throw new ArgumentNullException(nameof(workstationInfoModel));
            }

            var workstation = new Workstation()
            {
                Id = workstationInfoModel.Id,
                Name = workstationInfoModel.MachineName,
                Domain = workstationInfoModel.Domain,
                OS = workstationInfoModel.OsName,
                ClientVersion = workstationInfoModel.AppVersion,
                IP = workstationInfoModel.IP,
                LastSeen = DateTime.UtcNow,
                DepartmentId = null
            };

            _dbContext.Workstations.Add(workstation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task EditWorkstationInfoAsync(WorkstationInfoDto workstationInfoModel)
        {
            if (workstationInfoModel == null)
            {
                throw new ArgumentNullException(nameof(workstationInfoModel));
            }

            var workstation = await GetWorkstationByIdAsync(workstationInfoModel.Id);
            if (workstation == null)
            {
                throw new HESException(HESCode.WorkstationNotFound);
            }

            workstation.ClientVersion = workstationInfoModel.AppVersion;
            workstation.OS = workstationInfoModel.OsName;
            workstation.IP = workstationInfoModel.IP;
            workstation.LastSeen = DateTime.UtcNow;

            _dbContext.Workstations.Update(workstation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task EditWorkstationAsync(Workstation workstation)
        {
            if (workstation == null)
            {
                throw new ArgumentNullException(nameof(workstation));
            }

            _dbContext.Workstations.Update(workstation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ApproveWorkstationAsync(Workstation workstation)
        {
            if (workstation == null)
            {
                throw new ArgumentNullException(nameof(workstation));
            }

            workstation.Approved = true;

            _dbContext.Workstations.Update(workstation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnapproveWorkstationAsync(string workstationId)
        {
            if (string.IsNullOrWhiteSpace(workstationId))
            {
                throw new ArgumentNullException(nameof(workstationId));
            }

            var workstation = await GetWorkstationByIdAsync(workstationId);
            if (workstation == null)
            {
                throw new HESException(HESCode.WorkstationNotFound);
            }

            workstation.Approved = false;
            workstation.RFID = false;
            workstation.DepartmentId = null;

            _dbContext.Workstations.Update(workstation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteWorkstationAsync(string workstationId)
        {
            if (string.IsNullOrWhiteSpace(workstationId))
            {
                throw new ArgumentNullException(nameof(workstationId));
            }

            var workstation = await GetWorkstationByIdAsync(workstationId);
            if (workstation == null)
            {
                throw new HESException(HESCode.WorkstationNotFound);
            }

            _dbContext.Workstations.Remove(workstation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> CheckIsRFIDEnabledAsync(string workstationId)
        {
            var workstation = await GetWorkstationByIdAsync(workstationId);
            if (workstation == null)
            {
                throw new HESException(HESCode.WorkstationNotFound);
            }

            return workstation.RFID;
        }

        public async Task<bool> CheckIsApprovedAsync(string workstationId)
        {
            var workstation = await GetWorkstationByIdAsync(workstationId);
            if (workstation == null)
            {
                return false;
            }

            return workstation.Approved;
        }

        public void UnchangedWorkstation(Workstation workstation)
        {
            _dbContext.Unchanged(workstation);
        }

        #endregion

        #region WorkstationHardwareVaultPair

        public async Task<WorkstationHardwareVaultPair> GetWorkstationHardwareVaultPairByIdAsync(string pairId)
        {
            return await _dbContext.WorkstationHardwareVaultPairs
                .Include(d => d.HardwareVault.Employee.Department.Company)
                .Include(d => d.Workstation.Department.Company)
                .FirstOrDefaultAsync(x => x.Id == pairId);
        }

        public async Task<List<WorkstationHardwareVaultPair>> GetWorkstationHardwareVaultPairsAsync(DataLoadingOptions<WorkstationDetailsFilter> dataLoadingOptions)
        {
            return await WorkstationHardwareVaultPairQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetWorkstationHardwareVaultPairsCountAsync(DataLoadingOptions<WorkstationDetailsFilter> dataLoadingOptions)
        {
            return await WorkstationHardwareVaultPairQuery(dataLoadingOptions).CountAsync();
        }

        private IQueryable<WorkstationHardwareVaultPair> WorkstationHardwareVaultPairQuery(DataLoadingOptions<WorkstationDetailsFilter> dataLoadingOptions)
        {
            var query = _dbContext.WorkstationHardwareVaultPairs
                .Include(d => d.HardwareVault.Employee.Department.Company)
                .Where(d => d.WorkstationId == dataLoadingOptions.EntityId);

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.HardwareVaultId.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.HardwareVault.Employee.FirstName + " " + x.HardwareVault.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVault.Employee.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVault.Employee.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(WorkstationHardwareVaultPair.HardwareVault):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVaultId) : query.OrderByDescending(x => x.HardwareVaultId);
                    break;
                case nameof(WorkstationHardwareVaultPair.HardwareVault.Employee):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVault.Employee.FirstName).ThenBy(x => x.HardwareVault.Employee.LastName) : query.OrderByDescending(x => x.HardwareVault.Employee.FirstName).ThenByDescending(x => x.HardwareVault.Employee.LastName);
                    break;
                case nameof(WorkstationHardwareVaultPair.HardwareVault.Employee.Department.Company):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVault.Employee.Department.Company.Name) : query.OrderByDescending(x => x.HardwareVault.Employee.Department.Company.Name);
                    break;
                case nameof(WorkstationHardwareVaultPair.HardwareVault.Employee.Department):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVault.Employee.Department.Name) : query.OrderByDescending(x => x.HardwareVault.Employee.Department.Name);
                    break;
            }

            return query;
        }

        public async Task<WorkstationHardwareVaultPair> CreateWorkstationHardwareVaultPairAsync(string workstationId, string vaultId)
        {
            if (string.IsNullOrWhiteSpace(workstationId))
            {
                throw new ArgumentNullException(nameof(workstationId));
            }

            if (string.IsNullOrWhiteSpace(vaultId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            var pairExist = await _dbContext.ExistAsync<WorkstationHardwareVaultPair>(x => x.HardwareVaultId == vaultId && x.WorkstationId == workstationId);
            if (pairExist)
            {
                throw new HESException(HESCode.WorkstationHardwareVaultPairAlreadyExist);
            }

            var pair = new WorkstationHardwareVaultPair
            {
                WorkstationId = workstationId,
                HardwareVaultId = vaultId,
                LockProximity = 30,
                UnlockProximity = 70,
                LockTimeout = 5
            };

            var result = _dbContext.WorkstationHardwareVaultPairs.Add(pair);
            await _dbContext.SaveChangesAsync();

            return result.Entity;
        }

        public async Task DeleteWorkstationHardwareVaultPairAsync(string pairId)
        {
            if (string.IsNullOrWhiteSpace(pairId))
            {
                throw new ArgumentNullException(nameof(pairId));
            }

            var pair = await _dbContext.WorkstationHardwareVaultPairs.FindAsync(pairId);
            if (pair == null)
            {
                throw new HESException(HESCode.WorkstationHardwareVaultPairNotFound);
            }

            _dbContext.WorkstationHardwareVaultPairs.Remove(pair);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteWorkstationHardwareVaultPairsByVaultIdAsync(string vaultId)
        {
            var proximityVaults = _dbContext.WorkstationHardwareVaultPairs.Where(x => x.HardwareVaultId == vaultId);

            _dbContext.WorkstationHardwareVaultPairs.RemoveRange(proximityVaults);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<HwVaultProximitySettingsDto>> GetWorkstationHardwareVaultPairSettingsAsync(string workstationId)
        {
            var workstation = await GetWorkstationByIdAsync(workstationId);
            if (workstation == null)
            {
                throw new HESException(HESCode.WorkstationNotFound);
            }

            var settings = new List<HwVaultProximitySettingsDto>();

            var proximityDevices = await _dbContext.WorkstationHardwareVaultPairs
                .Include(d => d.HardwareVault)
                .Where(d => d.WorkstationId == workstationId)
                .AsNoTracking()
                .ToListAsync();

            if (workstation.Approved)
            {
                foreach (var proximity in proximityDevices)
                {
                    settings.Add(new HwVaultProximitySettingsDto()
                    {
                        SerialNo = proximity.HardwareVaultId,
                        Mac = proximity.HardwareVault.MAC,
                        LockProximity = proximity.LockProximity,
                        UnlockProximity = proximity.UnlockProximity,
                        LockTimeout = proximity.LockTimeout,
                    });
                }
            }

            return settings;
        }

        #endregion
    }
}