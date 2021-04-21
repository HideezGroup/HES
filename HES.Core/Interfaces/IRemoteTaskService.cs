using HES.Core.Entities;
using Hideez.SDK.Communication.Device;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task ExecuteRemoteTasks(string vaultId, Device remoteDevice, bool primaryAccountOnly);
        Task LinkVaultAsync(Device remoteDevice, HardwareVault vault);
        Task AccessVaultAsync(Device remoteDevice, HardwareVault vault);
        Task SuspendVaultAsync(Device remoteDevice, HardwareVault vault);
        Task WipeVaultAsync(Device remoteDevice, HardwareVault vault);
    }
}