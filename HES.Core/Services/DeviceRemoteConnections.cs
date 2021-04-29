using HES.Core.RemoteDeviceConnection;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
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
            public TaskCompletionSource<Device> Tcs { get; set; }
            public Device Device { get; set; }

            public RemoteDeviceDescription(IRemoteAppConnection appConnection)
            {
                AppConnection = appConnection;
            }
        }

        const int channelNo = 4;
        readonly string _deviceId;
        readonly ConcurrentDictionary<string, RemoteDeviceDescription> _appConnections = new();
        readonly ConcurrentDictionary<string, DeviceConnectionContainer> _connectionContainers = new();

        public bool IsDeviceConnectedToHost => _appConnections.Count > 0;

        public DeviceRemoteConnections(string deviceId)
        {
            _deviceId = deviceId;
        }

        // Device connected to the workstation, adding it to the list of the connected devices,
        // overwrite if already exists
        public void OnDeviceConnected(string workstationId, IRemoteAppConnection appConnection)
        {
            _appConnections.TryAdd(workstationId, new RemoteDeviceDescription(appConnection));
        }

        // Device disconnected from the workstation, removing it from the list of the connected devices
        public void OnDeviceDisconnected(string workstationId)
        {
            if (_appConnections.TryRemove(workstationId, out RemoteDeviceDescription descr))
            {
                descr.Device?.Shutdown();
            }
        }

        // Workstation disconnected from the server, if this device has connections to this workstation, close them
        public void OnAppHubDisconnected(string workstationId)
        {
            if (_appConnections.TryRemove(workstationId, out RemoteDeviceDescription descr))
            {
                descr.Device?.Shutdown();
            }
        }

        public async Task<Device> ConnectDevice(string workstationId)
        {
            RemoteDeviceDescription descr = null;
            if (workstationId == null)
            {
                // trying to connect to any workstation, first, look for that where Device is not empty
                descr = _appConnections.Values.Where(x => x.Device != null).FirstOrDefault();
                if (descr == null)
                {
                    descr = _appConnections.Values.FirstOrDefault();
                }
            }
            else
            {
                _appConnections.TryGetValue(workstationId, out descr);
            }

            if (descr == null)
            {
                throw new HideezException(HideezErrorCode.DeviceNotConnectedToAnyHost);
            }

            TaskCompletionSource<Device> tcs = null;
            lock (descr)
            {
                if (descr.Device != null)
                {
                    return descr.Device;
                }

                tcs = descr.Tcs;
                if (tcs == null)
                {
                    descr.Tcs = new TaskCompletionSource<Device>();
                }
            }

            if (tcs != null)
            {
                return await tcs.Task;
            }

            try
            {
                // Call Hideez Client to make remote channel
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
                        var connectionContainer = _connectionContainers.GetOrAdd(workstationId, (c) => new DeviceConnectionContainer(caller, _deviceId));
                        var deviceConnection = new SignalRRemoteDeviceConnection(connectionContainer);
                        var commandQueue = new CommandQueue(deviceConnection, null);

                        var deviceCommands = new HesDeviceCommands(connectionContainer);
                        var remoteDevice = new Device(commandQueue, channelNo, deviceCommands, null);
                        descr.Device = remoteDevice;

                        await remoteDevice.VerifyAndInitialize();

                        // Inform clients about connection ready
                        descr.Tcs.TrySetResult(remoteDevice);
                    }
                }
                catch (Exception ex)
                {
                    descr.Device = null;

                    // Inform clients about connection fail
                    descr.Tcs?.TrySetException(ex);
                }
            });
        }

        internal void OnDeviceHubDisconnected(string workstationId)
        {
            if (_appConnections.TryRemove(workstationId, out RemoteDeviceDescription descr))
            {
                descr.Device?.Shutdown();
            }

            _connectionContainers.TryRemove(workstationId, out DeviceConnectionContainer deviceConnection);
        }

        internal Device GetRemoteDevice(string workstationId)
        {
            _appConnections.TryGetValue(workstationId, out RemoteDeviceDescription descr);
            return descr?.Device;
        }

        internal DeviceConnectionContainer GetConnectionContainer(string workstationId)
        {
            _connectionContainers.TryGetValue(workstationId, out DeviceConnectionContainer connectionContainer);
            return connectionContainer;
        }

        internal Device GetFirstOrDefaultRemoteDevice()
        {
            var kvp = _appConnections.FirstOrDefault();
            if (kvp.Value == null)
            {
                return null;
            }
            return kvp.Value.Device;
        }

        internal string GetFirstOrDefaultWorkstation()
        {
            var kvp = _appConnections.FirstOrDefault();
            if (kvp.Key == null)
            {
                return null;
            }
            return kvp.Key;
        }
    }
}