using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteWorkstationConnectionsService : IRemoteWorkstationConnectionsService, IDisposable
    {
        static readonly ConcurrentDictionary<string, IRemoteAppConnection> _workstationConnections
                    = new ConcurrentDictionary<string, IRemoteAppConnection>();

        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly IEmployeeService _employeeService;
        private readonly IAccountService _accountService;
        private readonly IWorkstationService _workstationService;
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly ILogger<RemoteWorkstationConnectionsService> _logger;
        private readonly IAppSettingsService _appSettingsService;


        public RemoteWorkstationConnectionsService(IRemoteTaskService remoteTaskService,
                      IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IEmployeeService employeeService,
                      IAccountService accountService,
                      IWorkstationService workstationService,
                      IHardwareVaultService hardwareVaultService,                 
                      IWorkstationAuditService workstationAuditService,
                      ILogger<RemoteWorkstationConnectionsService> logger,              
                      IAppSettingsService appSettingsService)
        {
            _remoteTaskService = remoteTaskService;
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _employeeService = employeeService;
            _accountService = accountService;
            _workstationService = workstationService;
            _hardwareVaultService = hardwareVaultService;
            _workstationAuditService = workstationAuditService;
            _logger = logger;
            _appSettingsService = appSettingsService;
        }

        public async Task RegisterWorkstationInfoAsync(IRemoteAppConnection remoteAppConnection, WorkstationInfoDto workstationInfoDto)
        {
            if (workstationInfoDto == null)
                throw new ArgumentNullException(nameof(workstationInfoDto));

            _workstationConnections.AddOrUpdate(workstationInfoDto.Id, remoteAppConnection, (id, oldConnection) =>
            {
                return remoteAppConnection;
            });

            if (await _workstationService.ExistAsync(w => w.Id == workstationInfoDto.Id))
            {
                // Workstation exists, update information
                await _workstationService.UpdateWorkstationInfoAsync(workstationInfoDto);
            }
            else
            {
                // Workstation does not exist or name + domain was changed, create new
                await _workstationService.AddWorkstationAsync(workstationInfoDto);
                _logger.LogInformation($"New workstation {workstationInfoDto.MachineName} was added.");
            }

            await UpdateProximitySettingsAsync(workstationInfoDto.Id, await _workstationService.GetProximitySettingsAsync(workstationInfoDto.Id));
            await UpdateRfidStateAsync(workstationInfoDto.Id, await _workstationService.GetRfidStateAsync(workstationInfoDto.Id));
        }

        public async Task OnAppHubDisconnectedAsync(string workstationId)
        {
            _workstationConnections.TryRemove(workstationId, out IRemoteAppConnection _);

            await _workstationAuditService.CloseSessionAsync(workstationId);
        }

        public static IRemoteAppConnection FindWorkstationConnection(string workstationId)
        {
            _workstationConnections.TryGetValue(workstationId, out IRemoteAppConnection workstation);
            return workstation;
        }

        public static bool IsWorkstationConnected(string workstationId)
        {
            return _workstationConnections.ContainsKey(workstationId);
        }

        public async Task LockAllWorkstationsAsync(string userEmail)
        {
            foreach (var workstationConnection in _workstationConnections)
                await workstationConnection.Value.SetAlarmState(true);

            var alarmState = new AlarmState
            {
                IsAlarm = true,
                AdminName = userEmail ?? "undefined",
                Date = DateTime.UtcNow
            };

            await _appSettingsService.SetAlarmStateAsync(alarmState);
        }

        public async Task UnlockAllWorkstationsAsync(string userEmail)
        {
            var state = await _appSettingsService.GetAlarmStateAsync();
            if (state != null && !state.IsAlarm)
                return;

            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentNullException(nameof(userEmail));

            var alarmState = new AlarmState
            {
                IsAlarm = false,
                AdminName = userEmail,
                Date = DateTime.UtcNow
            };

            await _appSettingsService.SetAlarmStateAsync(alarmState);


            foreach (var workstationConnection in _workstationConnections)
                await workstationConnection.Value.SetAlarmState(false);
        }

        public static int WorkstationsOnlineCount()
        {
            return _workstationConnections.Count;
        }

        public async Task UpdateProximitySettingsAsync(string workstationId, IReadOnlyList<HwVaultProximitySettingsDto> proximitySettings)
        {
            var remoteAppConnection = FindWorkstationConnection(workstationId);
            if (remoteAppConnection == null)
                return;

            await remoteAppConnection.UpdateProximitySettings(proximitySettings);
        }

        public async Task UpdateRfidStateAsync(string workstationId, bool isEnabled)
        {
            var remoteAppConnection = FindWorkstationConnection(workstationId);
            if (remoteAppConnection == null)
                return;

            await remoteAppConnection.UpdateRFIDIndicatorState(isEnabled);
        }

        public async Task UpdateWorkstationApprovedAsync(string workstationId, bool isApproved)
        {
            var remoteAppConnection = FindWorkstationConnection(workstationId);
            if (remoteAppConnection == null)
                return;

            if (isApproved)
            {
                await remoteAppConnection.WorkstationApproved();
            }
            else
            {
                await remoteAppConnection.WorkstationUnapproved();
            }
        }

        public void Dispose()
        {
            _remoteTaskService.Dispose();
            _remoteDeviceConnectionsService.Dispose();
            _employeeService.Dispose();
            _accountService.Dispose();
            _workstationService.Dispose();
            _hardwareVaultService.Dispose();
            _workstationAuditService.Dispose();
            _appSettingsService.Dispose();
        }
    }
}