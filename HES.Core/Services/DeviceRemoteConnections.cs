using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceRemoteConnections
    {
        class RemoteDeviceDescription
        {
            public IRemoteAppConnection AppConnection { get; }
            public TaskCompletionSource<RemoteDevice> Tcs { get; set; }
            public RemoteDevice Device { get; set; }

            public RemoteDeviceDescription(IRemoteAppConnection appConnection)
            {
                AppConnection = appConnection;
            }
        }

        const int channelNo = 4;

        readonly string _deviceId;

        readonly ConcurrentDictionary<string, RemoteDeviceDescription> _appConnections
            = new ConcurrentDictionary<string, RemoteDeviceDescription>();

        public bool IsDeviceConnectedToHost => _appConnections.Count > 0;

        public DeviceRemoteConnections(string deviceId)
        {
            _deviceId = deviceId;
        }

        // device connected to the workstation, adding it to the list of the connected devices
        // overwrite if already exists
        public void OnDeviceConnected(string workstationId, IRemoteAppConnection appConnection)
        {
            //_appConnections.AddOrUpdate(workstationId, new RemoteDeviceDescription(appConnection), (conn, old) =>
            //{
            //    return new RemoteDeviceDescription(appConnection);
            //});
            _appConnections.TryAdd(workstationId, new RemoteDeviceDescription(appConnection));
        }

        // device disconnected from the workstation, removing it from the list of the connected devices
        public void OnDeviceDisconnected(string workstationId)
        {
            if (_appConnections.TryRemove(workstationId, out RemoteDeviceDescription descr))
            {
                descr.Device?.Shutdown(HideezErrorCode.DeviceDisconnected);
            }
        }

        // workstation disconnected from the server, if this device has connections to this workstation, close them
        public void OnAppHubDisconnected(string workstationId)
        {
            if (_appConnections.TryRemove(workstationId, out RemoteDeviceDescription descr))
            {
                descr.Device?.Shutdown(HideezErrorCode.HesAppHubDisconnected);
            }
        }

        public async Task<RemoteDevice> ConnectDevice(string workstationId)
        {
            RemoteDeviceDescription descr = null;
            if (workstationId == null)
            {
                // trying to connect to any workstation, first, look for that where Device is not empty
                descr = _appConnections.Values.Where(x => x.Device != null).FirstOrDefault();
                if (descr == null)
                    descr = _appConnections.Values.FirstOrDefault();
            }
            else
            {
                _appConnections.TryGetValue(workstationId, out descr);
            }

            if (descr == null)
                throw new HideezException(HideezErrorCode.DeviceNotConnectedToAnyHost);

            TaskCompletionSource<RemoteDevice> tcs = null;
            lock (descr)
            {
                if (descr.Device != null)
                    return descr.Device;

                tcs = descr.Tcs;
                if (tcs == null)
                    descr.Tcs = new TaskCompletionSource<RemoteDevice>();
            }

            if (tcs != null)
                return await tcs.Task;

            try
            {
                // call Hideez Client to make remote channel
                await descr.AppConnection.EstablishRemoteHwVaultConnection(_deviceId, channelNo);

                await descr.Tcs.Task.TimeoutAfter(20_000);

                return descr.Device;
            }
            catch (TimeoutException)
            {
                var ex = new HideezException(HideezErrorCode.RemoteConnectionTimedOut);
                descr.Tcs.TrySetException(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                descr.Tcs.TrySetException(ex);
                throw;
            }
            finally
            {
                lock (descr)
                {
                    descr.Tcs = null;
                }
            }
        }

        internal void OnDeviceHubConnected(string workstationId, IRemoteCommands caller)
        {
            Task.Run(async () =>
            {
                RemoteDeviceDescription descr = null;
                try
                {
                    _appConnections.TryGetValue(workstationId, out descr);
                    if (descr != null)
                    {
                        var remoteDevice = new RemoteDevice(_deviceId, channelNo, caller, null, SdkConfig.DefaultRemoteCommandTimeout, null, null);
                        descr.Device = remoteDevice;

                        await remoteDevice.VerifyAndInitialize(new System.Threading.CancellationToken());

                        // Inform clients about connection ready
                        descr.Tcs.TrySetResult(remoteDevice);
                    }
                }
                catch (Exception ex)
                {
                    descr.Device = null;

                    // inform clients about connection fail
                    descr.Tcs?.TrySetException(ex);
                }
            });
        }

        internal void OnDeviceHubDisconnected(string workstationId)
        {
            if (_appConnections.TryRemove(workstationId, out RemoteDeviceDescription descr))
            {
                descr.Device?.Shutdown(HideezErrorCode.HesDeviceHubDisconnected);
            }
        }

        internal RemoteDevice GetRemoteDevice(string workstationId)
        {
            _appConnections.TryGetValue(workstationId, out RemoteDeviceDescription descr);
            return descr?.Device;
        }

        internal RemoteDevice GetFirstOrDefaultRemoteDevice()
        {
            var kvp = _appConnections.FirstOrDefault();
            if (kvp.Value == null)
                return null;
            return kvp.Value.Device;
        }

        internal string GetFirstOrDefaultWorkstation()
        {
            var kvp = _appConnections.FirstOrDefault();
            if (kvp.Key == null)
                return null;
            return kvp.Key;
        }
    }
}