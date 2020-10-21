using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteDeviceConnectionsService : IRemoteDeviceConnectionsService, IDisposable
    {
        private static readonly ConcurrentDictionary<string, DeviceRemoteConnections> _deviceRemoteConnectionsList
            = new ConcurrentDictionary<string, DeviceRemoteConnections>();
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _devicesInProgress
            = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

        private readonly IServiceProvider _services;
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IEmployeeService _employeeService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ILogger<RemoteDeviceConnectionsService> _logger;

        public RemoteDeviceConnectionsService(IServiceProvider services,
                                              IHardwareVaultService hardwareVaultService,
                                              IRemoteTaskService remoteTaskService,
                                              IEmployeeService employeeService,
                                              IDataProtectionService dataProtectionService,
                                              ILogger<RemoteDeviceConnectionsService> logger)
        {
            _services = services;
            _hardwareVaultService = hardwareVaultService;
            _remoteTaskService = remoteTaskService;
            _employeeService = employeeService;
            _dataProtectionService = dataProtectionService;
            _logger = logger;
        }

        static DeviceRemoteConnections GetDeviceRemoteConnections(string deviceId)
        {
            return _deviceRemoteConnectionsList.GetOrAdd(deviceId, (x) =>
            {
                return new DeviceRemoteConnections(deviceId);
            });
        }

        // Device connected to the host (called by AppHub)
        public void OnDeviceConnected(string deviceId, string workstationId, IRemoteAppConnection appConnection)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceConnected(workstationId, appConnection);
        }

        // Device disconnected from the host (called by AppHub)
        public void OnDeviceDisconnected(string deviceId, string workstationId)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceDisconnected(workstationId);
        }

        // Device hub connected. That means we need to create RemoteDevice
        public void OnDeviceHubConnected(string deviceId, string workstationId, IRemoteCommands caller)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceHubConnected(workstationId, caller);
        }

        // Device hub disconnected
        public void OnDeviceHubDisconnected(string deviceId, string workstationId)
        {
            GetDeviceRemoteConnections(deviceId).OnDeviceHubDisconnected(workstationId);
        }

        // AppHub connected
        public void OnAppHubConnected(string workstationId, IRemoteAppConnection appConnection)
        {
        }

        // AppHub disconnected
        public void OnAppHubDisconnected(string workstationId)
        {
            foreach (var item in _deviceRemoteConnectionsList.Values)
            {
                item.OnAppHubDisconnected(workstationId);
            }
        }

        public static bool IsDeviceConnectedToHost(string deviceId)
        {
            return GetDeviceRemoteConnections(deviceId).IsDeviceConnectedToHost;
        }

        public Task<RemoteDevice> ConnectDevice(string deviceId, string workstationId)
        {
            _deviceRemoteConnectionsList.TryGetValue(deviceId, out DeviceRemoteConnections deviceRemoteConnections);
            if (deviceRemoteConnections == null || !deviceRemoteConnections.IsDeviceConnectedToHost)
                throw new HideezException(HideezErrorCode.DeviceNotConnectedToAnyHost);

            return deviceRemoteConnections.ConnectDevice(workstationId);
        }

        public RemoteDevice FindRemoteDevice(string deviceId, string workstationId)
        {
            return GetDeviceRemoteConnections(deviceId).GetRemoteDevice(workstationId);
        }

        public async Task<bool> CheckIsNeedUpdateHwVaultStatusAsync(HwVaultInfoFromClientDto dto)
        {
            var vault = await ValidateAndGetHardwareVaultAsync(dto.VaultSerialNo, dto.IsLinkRequired);

            switch (vault.Status)
            {
                case VaultStatus.Ready:
                    throw new HideezException(HideezErrorCode.HesDeviceNotAssignedToAnyUser);
                case VaultStatus.Reserved:
                    // Required Link
                    if (dto.IsLinkRequired)
                        return true;
                    // Transition to Active status
                    if (!dto.IsLocked)
                        return true;
                    // Transition to Locked status
                    if (dto.IsLocked && !dto.IsCanUnlock)
                        return true;
                    break;
                case VaultStatus.Active:
                    // Transition to Locked status
                    if (dto.IsLocked)
                        return true;
                    break;
                case VaultStatus.Locked:
                    throw new HideezException(HideezErrorCode.HesDeviceLocked);
                case VaultStatus.Suspended:
                    // Transition to Locked status
                    if (dto.IsLocked && !dto.IsCanUnlock)
                        return true;
                    // Transition to Active status
                    if (!dto.IsLocked)
                        return true;
                    break;
                case VaultStatus.Deactivated:
                    return true;
                case VaultStatus.Compromised:
                    throw new HideezException(HideezErrorCode.HesDeviceCompromised);
                default:
                    throw new Exception($"Unhandled vault status. ({vault.Status})");
            }

            return false;
        }

        private async Task<HardwareVault> ValidateAndGetHardwareVaultAsync(string vaultId, bool isLinkRequired)
        {
            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

            if (vault == null)
                throw new HideezException(HideezErrorCode.HesDeviceNotFound);

            if (!isLinkRequired && vault.MasterPassword == null)
                throw new HideezException(HideezErrorCode.HesDeviceLinkedToAnotherServer);

            if (isLinkRequired && vault.MasterPassword != null && vault.Status != VaultStatus.Reserved && vault.Status != VaultStatus.Deactivated)
                throw new HideezException(HideezErrorCode.HesVaultWasManuallyWiped);

            return vault;
        }

        public async Task UpdateHardwareVaultStatusAsync(string vaultId, string workstationId)
        {
            var remoteDevice = await ConnectDevice(vaultId, workstationId).TimeoutAfter(30_000);

            await remoteDevice.RefreshDeviceInfo();

            if (remoteDevice == null)
                throw new HideezException(HideezErrorCode.HesFailedEstablishRemoteDeviceConnection);

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

            if (vault == null)
                throw new HideezException(HideezErrorCode.HesDeviceNotFound);
            _logger.LogDebug($"[UpdateHardwareVaultStatusAsync] Status: {vault.Status} MasterPassword: {vault.MasterPassword}");
            switch (vault.Status)
            {
                case VaultStatus.Ready:
                    throw new HideezException(HideezErrorCode.HesDeviceNotAssignedToAnyUser);
                case VaultStatus.Reserved:
                    // Required Link
                    if (remoteDevice.AccessLevel.IsLinkRequired)
                    {
                        await _remoteTaskService.LinkVaultAsync(remoteDevice, vault);
                        _logger.LogDebug($"[LINK]({vault.Id}) MasterPassword: {vault.MasterPassword} Status:{vault.Status}");
                        break;
                    }
                    // Transition to Active status
                    if (!remoteDevice.AccessLevel.IsLinkRequired && !remoteDevice.IsLocked)
                    {
                        await _remoteTaskService.AccessVaultAsync(remoteDevice, vault);
                        _logger.LogDebug($"[ACCESS]({vault.Id}) MasterPassword: {vault.MasterPassword}  Status:{vault.Status}");
                        await _hardwareVaultService.SetActiveStatusAsync(vault);
                        _logger.LogDebug($"[ACTIVATE]({vault.Id}) MasterPassword: {vault.MasterPassword}  Status:{vault.Status}");
                        break;
                    }
                    // Transition to Locked status
                    if (remoteDevice.IsLocked && !remoteDevice.IsCanUnlock)
                    {
                        await _hardwareVaultService.SetLockedStatusAsync(vault);
                        _logger.LogDebug($"[LOCK]({vault.Id}) MasterPassword: {vault.MasterPassword}  Status:{vault.Status}");
                        break;
                    }
                    break;
                case VaultStatus.Active:
                    // Transition to Locked status
                    if (remoteDevice.IsLocked)
                    {
                        await _hardwareVaultService.SetLockedStatusAsync(vault);
                        _logger.LogDebug($"[LOCK]({vault.Id}) MasterPassword: {vault.MasterPassword}  Status:{vault.Status}");
                        break;
                    }
                    break;
                case VaultStatus.Locked:
                    throw new HideezException(HideezErrorCode.HesDeviceLocked);
                case VaultStatus.Suspended:
                    // Apply Suspend status
                    if (!vault.IsStatusApplied)
                    {
                        await _remoteTaskService.SuspendVaultAsync(remoteDevice, vault);
                        break;
                    }
                    else
                    {
                        // Transition to Locked status
                        if (remoteDevice.IsLocked && !remoteDevice.IsCanUnlock)
                        {
                            await _hardwareVaultService.SetLockedStatusAsync(vault);
                            break;
                        }
                        // Transition to Active status
                        if (!remoteDevice.IsLocked)
                        {
                            _logger.LogDebug($"[ACTIVE]({vault.Id}) MasterPassword: {vault.MasterPassword} in Status:{vault.Status}");
                            await _hardwareVaultService.SetActiveStatusAsync(vault);
                            break;
                        }
                    }
                    break;
                case VaultStatus.Deactivated:
                    _logger.LogDebug($"[WIPE]({vault.Id}) MasterPassword: {vault.MasterPassword}  Status:{vault.Status}");
                    await _remoteTaskService.WipeVaultAsync(remoteDevice, vault);
                    await _hardwareVaultService.SetReadyStatusAsync(vault);
                    throw new HideezException(HideezErrorCode.DeviceHasBeenWiped);
                case VaultStatus.Compromised:
                    throw new HideezException(HideezErrorCode.HesDeviceCompromised);
                default:
                    throw new Exception($"Unhandled vault status. ({vault.Status})");
            }
        }

        public void StartUpdateHardwareVaultStatus(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            if (!IsDeviceConnectedToHost(vaultId))
                return;

            var scope = _services.CreateScope();

            Task.Run(async () =>
            {
                try
                {
                    var remoteDeviceConnectionsService = scope.ServiceProvider.GetRequiredService<IRemoteDeviceConnectionsService>();
                    await remoteDeviceConnectionsService.UpdateHardwareVaultStatusAsync(vaultId, workstationId: null);
                }
                catch (HideezException ex)
                {
                    _logger.LogInformation($"[{vaultId}] {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[{vaultId}] {ex.Message}");
                }
                finally
                {
                    scope.Dispose();
                }
            });
        }

        public async Task CheckPassphraseAsync(string vaultId, string workstationId)
        {
            var remoteDevice = await ConnectDevice(vaultId, workstationId).TimeoutAfter(30_000);

            await remoteDevice.RefreshDeviceInfo();

            if (remoteDevice == null)
                throw new HideezException(HideezErrorCode.HesFailedEstablishRemoteDeviceConnection);

            var vault = await ValidateAndGetHardwareVaultAsync(vaultId, remoteDevice.AccessLevel.IsLinkRequired);
            _logger.LogDebug($"[CheckPassphrase] MasterPassword: {vault.MasterPassword}");
            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));

            await remoteDevice.CheckPassphrase(key);
        }

        public async Task SyncHardwareVaults(string vaultId)
        {
            try
            {
                var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

                if (vault.Status != VaultStatus.Active || vault.NeedSync || vault.EmployeeId == null)
                    return;

                var employee = await _employeeService.GetEmployeeByIdAsync(vault.EmployeeId);

                var employeeVaults = employee.HardwareVaults.Where(x => x.Id != vaultId && x.IsOnline && x.Timestamp != vault.Timestamp && x.Status == VaultStatus.Active && !x.NeedSync).ToList();
                foreach (var employeeVault in employeeVaults)
                {
                    var currentVaultSync = vault.Timestamp < employeeVault.Timestamp ? true : false;

                    var firstWorkstationId = GetDeviceRemoteConnections(vault.Id).GetFirstOrDefaultWorkstation();
                    var secondWorkstationId = GetDeviceRemoteConnections(employeeVault.Id).GetFirstOrDefaultWorkstation();

                    if (firstWorkstationId == null || secondWorkstationId == null)
                        return;

                    var firstRemoteDeviceTask = ConnectDevice(vault.Id, firstWorkstationId).TimeoutAfter(30_000);
                    var secondRemoteDeviceTask = ConnectDevice(employeeVault.Id, secondWorkstationId).TimeoutAfter(30_000);
                    await Task.WhenAll(firstRemoteDeviceTask, secondRemoteDeviceTask);

                    if (firstRemoteDeviceTask.Result == null || secondRemoteDeviceTask.Result == null)
                        return;

                    var firstVaultKey = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));
                    await firstRemoteDeviceTask.Result.CheckPassphrase(firstVaultKey);

                    var secondVaultKey = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(employeeVault.MasterPassword));
                    await secondRemoteDeviceTask.Result.CheckPassphrase(secondVaultKey);

                    IRemoteAppConnection appConnection;
                    string lockedVaultStorage;

                    if (currentVaultSync)
                    {
                        appConnection = RemoteWorkstationConnectionsService.FindWorkstationConnection(firstWorkstationId);

                        if (appConnection == null)
                            return;

                        lockedVaultStorage = vault.Id;
                    }
                    else
                    {
                        appConnection = RemoteWorkstationConnectionsService.FindWorkstationConnection(secondWorkstationId);

                        if (appConnection == null)
                            return;

                        lockedVaultStorage = employeeVault.Id;
                    }

                    await appConnection.LockHwVaultStorage(lockedVaultStorage);
                    try
                    {
                        await new DeviceStorageReplicator(firstRemoteDeviceTask.Result, secondRemoteDeviceTask.Result, null).Start();
                    }
                    finally
                    {
                        await appConnection.LiftHwVaultStorageLock(lockedVaultStorage);
                    }

                    if (vault.Timestamp > employeeVault.Timestamp)
                    {
                        await _hardwareVaultService.UpdateTimestampAsync(employeeVault, vault.Timestamp);
                    }
                    else
                    {
                        await _hardwareVaultService.UpdateTimestampAsync(vault, employeeVault.Timestamp);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sync Hardware Vaults - {ex.Message}");
            }
        }

        public async Task UpdateHardwareVaultAccountsAsync(string vaultId, string workstationId, bool onlyOsAccounts)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var isNew = false;
            var tcs = _devicesInProgress.GetOrAdd(vaultId, (x) =>
            {
                isNew = true;
                return new TaskCompletionSource<bool>();
            });

            if (!isNew)
            {
                await tcs.Task;
                return;
            }

            try
            {
                var remoteDevice = await ConnectDevice(vaultId, workstationId).TimeoutAfter(30_000);
                if (remoteDevice == null)
                    throw new HideezException(HideezErrorCode.HesFailedEstablishRemoteDeviceConnection);

                await _remoteTaskService.ExecuteRemoteTasks(vaultId, remoteDevice, onlyOsAccounts);

                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                throw;
            }
            finally
            {
                _devicesInProgress.TryRemove(vaultId, out TaskCompletionSource<bool> _);
            }
        }

        public void StartUpdateHardwareVaultAccounts(IList<string> vaultIds, bool onlyOsAccounts = false)
        {
            foreach (var vaultId in vaultIds)
            {
                StartUpdateHardwareVaultAccounts(vaultId, onlyOsAccounts);
            }
        }

        public void StartUpdateHardwareVaultAccounts(string vaultId, bool onlyOsAccounts = false)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            if (!IsDeviceConnectedToHost(vaultId))
                return;

            var scope = _services.CreateScope();

            Task.Run(async () =>
            {
                try
                {
                    var remoteDeviceConnectionsService = scope.ServiceProvider.GetRequiredService<IRemoteDeviceConnectionsService>();
                    await remoteDeviceConnectionsService.UpdateHardwareVaultAccountsAsync(vaultId, workstationId: null, onlyOsAccounts);
                }
                catch (HideezException ex)
                {
                    _logger.LogInformation($"[{vaultId}] {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[{vaultId}] {ex.Message}");
                }
                finally
                {
                    scope.Dispose();
                }
            });
        }

        public void Dispose()
        {
            _hardwareVaultService.Dispose();
            _remoteTaskService.Dispose();
            _employeeService.Dispose();
        }
    }
}