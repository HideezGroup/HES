using Hideez.SDK.Communication.Utils;
using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Device;

namespace HES.Core.RemoteDeviceConnection.Tasks
{
    public class RemoteGetRootKeyProc
    {
        readonly DeviceConnectionContainer _connectionContainer;
        readonly TaskCompletionSource<DeviceCommandReplyResult> _tcs = new TaskCompletionSource<DeviceCommandReplyResult>();

        public RemoteGetRootKeyProc(DeviceConnectionContainer connectionContainer)
        {
            _connectionContainer = connectionContainer;
        }

        public async Task<DeviceCommandReplyResult> Run(int timeout)
        {
            try
            {
                _connectionContainer.OnGetRootKeyCommandResponse += DeviceHub_OnGetRootKeyCommandResponse;

                await _connectionContainer.Caller.SendGetRootKeyCommand();

                return await _tcs.Task.TimeoutAfter(timeout);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
                return await _tcs.Task;
            }
            finally
            {
                _connectionContainer.OnGetRootKeyCommandResponse -= DeviceHub_OnGetRootKeyCommandResponse;
            }
        }

        private void DeviceHub_OnGetRootKeyCommandResponse(object sender, DeviceCommandReplyResultArgs e)
        {
            if (e.DeviceId == _connectionContainer.DeviceId)
            {
                if (!string.IsNullOrEmpty(e.Error))
                {
                    _tcs.TrySetException(new Exception(e.Error));
                    return;
                }

                _tcs.TrySetResult(e.DeviceCommandReplyResult);
            }
        }
    }
}
