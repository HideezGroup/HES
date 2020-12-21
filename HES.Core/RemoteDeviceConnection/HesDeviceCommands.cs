using HES.Core.RemoteDeviceConnection.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using System.Threading.Tasks;

namespace HES.Core.RemoteDeviceConnection
{
    public class HesDeviceCommands: IDeviceCommands
    {
        private readonly DeviceConnectionContainer _connectionContainer;

        public HesDeviceCommands(DeviceConnectionContainer connectionContainer)
        {
            _connectionContainer = connectionContainer;
        }

        public Task<DeviceCommandReplyResult> GetRootKey()
        {
            var res = new RemoteGetRootKeyProc(_connectionContainer).Run(SdkConfig.HesRequestTimeout);
            return res;
        }

        public async Task ResetEncryption(byte channelNo)
        {
            await _connectionContainer.Caller.SendResetChannel(channelNo);
        }

        public Task<DeviceCommandReplyResult> VerifyEncryption(byte[] pubKeyH, byte[] nonceH, byte verifyChannelNo)
        {
            var res = new RemoteVerifyEncryptionProc(_connectionContainer, pubKeyH, nonceH, verifyChannelNo).Run(SdkConfig.HesRequestTimeout);
            return res;
        }
    }
}
