using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using System;

namespace HES.Core.RemoteDeviceConnection
{
    public class DeviceConnectionContainer
    {
        public event EventHandler<DeviceCommandReplyResultArgs> OnVerifyCommandResponse;
        public event EventHandler<DeviceCommandReplyResultArgs> OnGetRootKeyCommandResponse;
        public event EventHandler<MessageBuffer> OnRemoteCommandResponse;

        public IRemoteCommands Caller { get; }
        public string DeviceId { get; }

        public DeviceConnectionContainer(IRemoteCommands caller, string deviceId)
        {
            Caller = caller;
            DeviceId = deviceId;
        }

        public void SetVerifyResponse(DeviceCommandReplyResult deviceCommandReplyResult, string error)
        {
            OnVerifyCommandResponse?.Invoke(this, new DeviceCommandReplyResultArgs(deviceCommandReplyResult, DeviceId, error));
        }

        public void SetGetRootKeyResponse(DeviceCommandReplyResult deviceCommandReplyResult, string error)
        {
            OnGetRootKeyCommandResponse?.Invoke(this, new DeviceCommandReplyResultArgs(deviceCommandReplyResult, DeviceId, error));
        }

        public void SetRemoteCommandResponse(MessageBuffer data, string error)
        {
            OnRemoteCommandResponse?.Invoke(this, data);
        }
    }

    public class DeviceCommandReplyResultArgs
    {
        public DeviceCommandReplyResult DeviceCommandReplyResult { get; }
        public string DeviceId { get; }
        public string Error { get; }

        public DeviceCommandReplyResultArgs(DeviceCommandReplyResult deviceCommandReplyResult, string deviceId, string error)
        {
            DeviceCommandReplyResult = deviceCommandReplyResult;
            DeviceId = deviceId;
            Error = error;
        }
    }
}
