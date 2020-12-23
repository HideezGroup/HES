using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Utils;
using System;
using System.Threading.Tasks;

namespace HES.Core.RemoteDeviceConnection.Tasks
{
    public class RemoteVerifyEncryptionProc
    {
        readonly DeviceConnectionContainer _connectionContainer;
        readonly byte[] _pubKeyH;
        readonly byte[] _nonceH;
        readonly byte _verifyChannelNo;
        readonly TaskCompletionSource<DeviceCommandReplyResult> _tcs = new TaskCompletionSource<DeviceCommandReplyResult>();

        public RemoteVerifyEncryptionProc(DeviceConnectionContainer connectionContainer, byte[] pubKeyH, byte[] nonceH, byte verifyChannelNo)
        {
            _connectionContainer = connectionContainer;
            _pubKeyH = pubKeyH;
            _nonceH = nonceH;
            _verifyChannelNo = verifyChannelNo;
        }

        public async Task<DeviceCommandReplyResult> Run(int timeout)
        {
            try
            {
                _connectionContainer.OnVerifyCommandResponse += DeviceHub_OnVerifyCommandResponse;

                await _connectionContainer.Caller.SendVerifyCommand(_pubKeyH, _nonceH, _verifyChannelNo);

                return await _tcs.Task.TimeoutAfter(timeout);
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
                return await _tcs.Task;
            }
            finally
            {
                _connectionContainer.OnVerifyCommandResponse -= DeviceHub_OnVerifyCommandResponse;
            }
        }

        private void DeviceHub_OnVerifyCommandResponse(object sender, DeviceCommandReplyResultArgs e)
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
