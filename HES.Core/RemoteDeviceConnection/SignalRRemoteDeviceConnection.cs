using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using System;
using System.Threading.Tasks;

namespace HES.Core.RemoteDeviceConnection
{
    public class SignalRRemoteDeviceConnection : IConnectionController
    {
        readonly DeviceConnectionContainer _connectionContainer;

        public string Id { get; }

        public string Name { get; }

        public ConnectionState State => ConnectionState.Connected;

        public string Mac => "00:00:00:00:00:00";

        public IConnection Connection => throw new NotImplementedException();

        public event EventHandler<MessageBuffer> ResponseReceived;
        public event EventHandler OperationCancelled;
        public event EventHandler<byte[]> DeviceStateChanged;
        public event EventHandler DeviceIsBusy;
        public event EventHandler<FwWipeStatus> WipeFinished;
        public event EventHandler ConnectionStateChanged;

        public SignalRRemoteDeviceConnection(DeviceConnectionContainer connectionContainer)
        {
            Id = connectionContainer.DeviceId;
            Name = $"RemoteDevice ({Id})";

            _connectionContainer = connectionContainer;
            _connectionContainer.OnRemoteCommandResponse += DeviceHub_OnRemoteCommandResponse;
        }

        private void DeviceHub_OnRemoteCommandResponse(object sender, MessageBuffer data)
        {
            ResponseReceived?.Invoke(this, data);
        }

        public bool IsBoot()
        {
            return false;
        }

        public async Task SendRequestAsync(EncryptedRequest request)
        {
            await _connectionContainer.Caller.SendRemoteCommand(request);
        }

        public async Task SendRequestAsync(ControlRequest request)
        {
            await _connectionContainer.Caller.SendControlRemoteCommand(request);
        }
    }
}
