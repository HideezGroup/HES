﻿using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IRemoteWorkstationConnectionsService : IDisposable
    {
        Task LockAllWorkstationsAsync(string userEmail);
        Task UnlockAllWorkstationsAsync(string userEmail);
        Task RegisterWorkstationInfoAsync(IRemoteAppConnection remoteAppConnection, WorkstationInfoDto workstationInfo);
        Task OnAppHubDisconnectedAsync(string workstationId);
        Task UpdateProximitySettingsAsync(string workstationId, IReadOnlyList<HwVaultProximitySettingsDto> proximitySettings);
        Task UpdateRfidStateAsync(string workstationId, bool isEnabled);
        Task UpdateWorkstationApprovedAsync(string workstationId, bool isApproved);
    }
}