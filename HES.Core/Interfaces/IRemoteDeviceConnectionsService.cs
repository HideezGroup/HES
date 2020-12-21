using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HES.Core.RemoteDeviceConnection;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;

namespace HES.Core.Interfaces
{
    public interface IRemoteDeviceConnectionsService : IDisposable
    {
        void OnDeviceHubConnected(string deviceId, string workstationId, IRemoteCommands caller);
        void OnDeviceHubDisconnected(string deviceId, string workstationId);

        void OnAppHubConnected(string workstationId, IRemoteAppConnection appConnection);
        void OnAppHubDisconnected(string workstationId);

        // Received via AppHub
        void OnDeviceConnected(string deviceId, string workstationId, IRemoteAppConnection appConnection);
        void OnDeviceDisconnected(string deviceId, string workstationId);

        Task<Device> ConnectDevice(string deviceId, string workstationId);
        DeviceConnectionContainer FindConnectionContainer(string deviceId, string workstationId);

        Task<bool> CheckIsNeedUpdateHwVaultStatusAsync(HwVaultInfoFromClientDto dto);
        Task UpdateHardwareVaultStatusAsync(string vaultId, string workstationId);
        void StartUpdateHardwareVaultStatus(string vaultId);
        Task CheckPassphraseAsync(string vaultId, string workstationId);
        Task SyncHardwareVaults(string vaultId);
        Task UpdateHardwareVaultAccountsAsync(string vaultId, string workstationId, bool onlyOsAccounts);
        void StartUpdateHardwareVaultAccounts(IList<string> vaultIds, bool onlyOsAccounts = false);
        void StartUpdateHardwareVaultAccounts(string vaultId, bool onlyOsAccounts = false);
    }
}
