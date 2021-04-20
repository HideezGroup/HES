﻿using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class AppHub : Hub<IRemoteAppConnection>
    {
        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IWorkstationService _workstationService;
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IEmployeeService _employeeService;
        private readonly ILicenseService _licenseService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IPageSyncService _synchronizationService;
        private readonly ILogger<AppHub> _logger;

        public AppHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                      IWorkstationAuditService workstationAuditService,
                      IWorkstationService workstationService,
                      IHardwareVaultService hardwareVaultService,
                      IEmployeeService employeeService,
                      ILicenseService licenseService,
                      IAppSettingsService appSettingsService,                    
                      IPageSyncService synchronizationService,
                      ILogger<AppHub> logger)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _workstationAuditService = workstationAuditService;
            _workstationService = workstationService;
            _hardwareVaultService = hardwareVaultService;
            _employeeService = employeeService;
            _licenseService = licenseService;
            _appSettingsService = appSettingsService;   
            _synchronizationService = synchronizationService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                string workstationId = httpContext.Request.Headers["WorkstationId"].ToString();

                if (string.IsNullOrWhiteSpace(workstationId))
                    throw new Exception($"AppHub.OnConnectedAsync - httpContext.Request.Headers does not contain WorkstationId");

                _remoteDeviceConnectionsService.OnAppHubConnected(workstationId, Clients.Caller);
                Context.Items.Add("WorkstationId", workstationId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var workstationId = GetWorkstationId();

                _remoteDeviceConnectionsService.OnAppHubDisconnected(workstationId);
                await _remoteWorkstationConnectionsService.OnAppHubDisconnectedAsync(workstationId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task ValidateConnectionAsync()
        {
            // Check workstation approving
            var approved = await _workstationService.CheckIsApprovedAsync(GetWorkstationId());
            if (!approved)
                throw new HideezException(HideezErrorCode.HesWorkstationNotApproved);

            // Check alarm enabling
            var alarmState = await _appSettingsService.GetAlarmStateAsync();
            if (alarmState != null && alarmState.IsAlarm)
                throw new HideezException(HideezErrorCode.HesAlarm);
        }

        #region Workstation

        // Incoming request
        public async Task<HesResponse> RegisterWorkstationInfo(WorkstationInfoDto workstationInfo)
        {
            try
            {
                // Add or Update workstation info
                await _remoteWorkstationConnectionsService.RegisterWorkstationInfoAsync(Clients.Caller, workstationInfo);

                var alarmState = await _appSettingsService.GetAlarmStateAsync();
                var isAlarm = alarmState != null && alarmState.IsAlarm;

                // Update alarm trigger if client was offline            
                if (workstationInfo.IsAlarmTurnOn && !isAlarm)
                    await Clients.Caller.SetAlarmState(false);

                await ValidateConnectionAsync();

                return HesResponse.Ok;
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{workstationInfo?.MachineName}] {ex.Message}");
                return new HesResponse(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{workstationInfo?.MachineName}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse> SaveClientEvents(WorkstationEventDto[] workstationEventsDto)
        {
            try
            {
                if (workstationEventsDto == null)
                    throw new ArgumentNullException(nameof(workstationEventsDto));

                await ValidateConnectionAsync();

                foreach (var eventDto in workstationEventsDto)
                {
                    try
                    {
                        await _workstationAuditService.AddEventDtoAsync(eventDto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[Add Event] {ex.Message}");
                    }

                    try
                    {
                        await _workstationAuditService.AddOrUpdateWorkstationSession(eventDto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[Add/Update Session] {ex.Message}");
                    }
                }

                return HesResponse.Ok;
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{workstationEventsDto?.FirstOrDefault()?.WorkstationId}] {ex.Message}");
                return new HesResponse(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{workstationEventsDto?.FirstOrDefault()?.WorkstationId}] {ex.Message}");
                return new HesResponse(ex);
            }
        }

        private string GetWorkstationId()
        {
            if (Context.Items.TryGetValue("WorkstationId", out object workstationId))
                return (string)workstationId;
            else
                throw new Exception("AppHub does not contain WorkstationId!");
        }

        #endregion

        #region HwVault

        // Incoming request
        public async Task<HesResponse<HwVaultShortInfoFromHesDto>> GetHwVaultInfoByRfid(string rfid)
        {
            try
            {
                await ValidateConnectionAsync();

                var vault = await _hardwareVaultService
                    .VaultQuery()
                    .Include(d => d.Employee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.RFID == rfid);

                var info = new HwVaultShortInfoFromHesDto()
                {
                    OwnerName = vault.Employee?.FullName,
                    OwnerEmail = vault.Employee?.Email,
                    VaultMac = vault.MAC,
                    VaultSerialNo = vault.Id,
                    VaultRfid = vault.RFID
                };

                return new HesResponse<HwVaultShortInfoFromHesDto>(info);
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[RFID {rfid}] {ex.Message}");
                return new HesResponse<HwVaultShortInfoFromHesDto>(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<HwVaultShortInfoFromHesDto>(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse<HwVaultShortInfoFromHesDto>> GetHwVaultInfoByMac(string mac)
        {
            try
            {
                await ValidateConnectionAsync();

                var vault = await _hardwareVaultService
                    .VaultQuery()
                    .Include(d => d.Employee)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MAC == mac);

                var info = new HwVaultShortInfoFromHesDto()
                {
                    OwnerName = vault.Employee?.FullName,
                    OwnerEmail = vault.Employee?.Email,
                    VaultMac = vault.MAC,
                    VaultSerialNo = vault.Id,
                    VaultRfid = vault.RFID
                };

                return new HesResponse<HwVaultShortInfoFromHesDto>(info);
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[MAC {mac}] {ex.Message}");
                return new HesResponse<HwVaultShortInfoFromHesDto>(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<HwVaultShortInfoFromHesDto>(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse<HwVaultShortInfoFromHesDto>> GetHwVaultInfoBySerialNo(string serialNo)
        {
            try
            {
                await ValidateConnectionAsync();

                var vault = await _hardwareVaultService
                     .VaultQuery()
                     .Include(d => d.Employee)
                     .AsNoTracking()
                     .FirstOrDefaultAsync(d => d.Id == serialNo);

                var info = new HwVaultShortInfoFromHesDto()
                {
                    OwnerName = vault.Employee?.FullName,
                    OwnerEmail = vault.Employee?.Email,
                    VaultMac = vault.MAC,
                    VaultSerialNo = vault.Id,
                    VaultRfid = vault.RFID
                };

                return new HesResponse<HwVaultShortInfoFromHesDto>(info);
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{serialNo}] {ex.Message}");
                return new HesResponse<HwVaultShortInfoFromHesDto>(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<HwVaultShortInfoFromHesDto>(ex);
            }
        }

        private async Task<HwVaultInfoFromHesDto> GetHardwareVaultInfoAsync(HwVaultInfoFromClientDto dto)
        {
            var vault = await _hardwareVaultService.GetVaultByIdAsync(dto.VaultSerialNo);

            if (vault == null)
                throw new HideezException(HideezErrorCode.HesDeviceNotFound);
                       
            return new HwVaultInfoFromHesDto()
            {
                OwnerName = vault.Employee?.FullName,
                OwnerEmail = vault.Employee?.Email,
                VaultMac = vault.MAC,
                VaultSerialNo = vault.Id,
                VaultRfid = vault.RFID,
                NeedUpdateLicense = vault.HasNewLicense,
                NeedStateUpdate = await _remoteDeviceConnectionsService.CheckIsNeedUpdateHwVaultStatusAsync(dto),
                NeedUpdateOSAccounts = vault.HardwareVaultTasks.Any(x => x.Operation == TaskOperation.Primary || x.AccountId == vault.Employee.PrimaryAccountId),
                NeedUpdateNonOSAccounts = vault.HardwareVaultTasks.Any(x => x.Operation != TaskOperation.Primary && x.AccountId != vault.Employee.PrimaryAccountId)
            };
        }

        // Incoming request
        public async Task<HesResponse<HwVaultInfoFromHesDto>> UpdateHwVaultProperties(HwVaultInfoFromClientDto dto, bool returnInfo = false)
        {
            try
            {
                await ValidateConnectionAsync();

                await _hardwareVaultService.UpdateVaultInfoAsync(dto);

                switch (dto.ConnectionState)
                {
                    case HwVaultConnectionState.Offline:
                        _remoteDeviceConnectionsService.OnDeviceDisconnected(dto.VaultSerialNo, GetWorkstationId());
                        await _employeeService.UpdateLastSeenAsync(dto.VaultSerialNo);
                        await InvokeVaultStateChangedAsync(dto.VaultSerialNo);
                        break;
                    case HwVaultConnectionState.Initializing:
                        _remoteDeviceConnectionsService.OnDeviceConnected(dto.VaultSerialNo, GetWorkstationId(), Clients.Caller);
                        break;
                    case HwVaultConnectionState.Finalizing:
                        break;
                    case HwVaultConnectionState.Online:
                        await InvokeVaultStateChangedAsync(dto.VaultSerialNo);
                        break;
                }

                HwVaultInfoFromHesDto info = null;

                if (returnInfo)
                {
                    info = await GetHardwareVaultInfoAsync(dto);

                    return new HesResponse<HwVaultInfoFromHesDto>(info);
                }

                return new HesResponse<HwVaultInfoFromHesDto>(info);
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{dto.VaultSerialNo}] {ex.Message}");
                return new HesResponse<HwVaultInfoFromHesDto>(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<HwVaultInfoFromHesDto>(ex);
            }
        }

        private async Task InvokeVaultStateChangedAsync(string vaultId)
        {
            // Update
            await _synchronizationService.HardwareVaultStateChanged(vaultId);
        }

        // Incoming request
        public async Task<HesResponse<HwVaultInfoFromHesDto>> UpdateHwVaultStatus(HwVaultInfoFromClientDto dto)
        {
            try
            {
                await ValidateConnectionAsync();

                await _remoteDeviceConnectionsService.UpdateHardwareVaultStatusAsync(dto.VaultSerialNo, GetWorkstationId());

                var info = await GetHardwareVaultInfoAsync(dto);

                return new HesResponse<HwVaultInfoFromHesDto>(info);
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{dto.VaultSerialNo}] {ex.Message}");
                return new HesResponse<HwVaultInfoFromHesDto>(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<HwVaultInfoFromHesDto>(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse> AuthHwVault(string serialNo)
        {
            try
            {
                await ValidateConnectionAsync();

                await _remoteDeviceConnectionsService.CheckPassphraseAsync(serialNo, GetWorkstationId());

                return HesResponse.Ok;
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{serialNo}] {ex.Message}");
                return new HesResponse(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse> UpdateHwVaultAccounts(string serialNo, bool onlyOsAccounts)
        {
            try
            {
                await ValidateConnectionAsync();

                await _remoteDeviceConnectionsService.UpdateHardwareVaultAccountsAsync(serialNo, GetWorkstationId(), onlyOsAccounts);

                return HesResponse.Ok;
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{serialNo}] {ex.Message}");
                return new HesResponse(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse<IList<HwVaultLicenseDto>>> GetNewHwVaultLicenses(string vaultId)
        {
            try
            {
                await ValidateConnectionAsync();

                var licenses = await _licenseService.GetNewLicensesByHardwareVaultIdAsync(vaultId);

                var licensesDto = new List<HwVaultLicenseDto>();

                foreach (var license in licenses)
                {
                    licensesDto.Add(new HwVaultLicenseDto
                    {
                        Id = license.Id,
                        DeviceId = license.HardwareVaultId,
                        ImportedAt = license.ImportedAt,
                        AppliedAt = license.AppliedAt,
                        EndDate = license.EndDate,
                        LicenseOrderId = license.LicenseOrderId,
                        Data = license.Data,
                    });
                }

                return new HesResponse<IList<HwVaultLicenseDto>>(licensesDto);
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{vaultId}] {ex.Message}");
                return new HesResponse<IList<HwVaultLicenseDto>>(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<IList<HwVaultLicenseDto>>(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse> HwVaultLicenseApplied(string vaultId, string licenseId)
        {
            try
            {
                await ValidateConnectionAsync();
                await _licenseService.ChangeLicenseAppliedAsync(vaultId, licenseId);
                return HesResponse.Ok;
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{vaultId}] {ex.Message}");
                return new HesResponse(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public async Task<HesResponse<IList<HwVaultLicenseDto>>> GetHwVaultLicenses(string vaultId)
        {
            try
            {
                await ValidateConnectionAsync();

                var licenses = await _licenseService.GetActiveLicensesAsync(vaultId);

                var licensesDto = new List<HwVaultLicenseDto>();

                foreach (var license in licenses)
                {
                    licensesDto.Add(new HwVaultLicenseDto
                    {
                        Id = license.Id,
                        DeviceId = license.HardwareVaultId,
                        ImportedAt = license.ImportedAt,
                        AppliedAt = license.AppliedAt,
                        EndDate = license.EndDate,
                        LicenseOrderId = license.LicenseOrderId,
                        Data = license.Data,
                    });
                }

                return new HesResponse<IList<HwVaultLicenseDto>>(licensesDto);
            }
            catch (HideezException ex)
            {
                _logger.LogInformation($"[{vaultId}] {ex.Message}");
                return new HesResponse<IList<HwVaultLicenseDto>>(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse<IList<HwVaultLicenseDto>>(ex);
            }
        }

        #endregion
    }
}