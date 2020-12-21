using HES.Core.Interfaces;
using HES.Core.RemoteDeviceConnection;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Core.Hubs
{
    public class DeviceHub : Hub<IRemoteCommands>
    {
        readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        readonly ILogger<DeviceHub> _logger;

        public DeviceHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                         ILogger<DeviceHub> logger)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _logger = logger;
        }

        private string GetWorkstationId()
        {
            if (Context.Items.TryGetValue("WorkstationId", out object workstationId))
                return (string)workstationId;
            else
            {
                throw new Exception("DeviceHub does not contain WorkstationId!");
            }
        }

        private string GetDeviceId()
        {
            if (Context.Items.TryGetValue("DeviceId", out object deviceId))
                return (string)deviceId;
            else
            {
                throw new Exception("DeviceHub does not contain DeviceId!");
            }
        }

        // Gets a device from the context
        private DeviceConnectionContainer GetDeviceConnectionContainer()
        {
            var connectionContainer = _remoteDeviceConnectionsService.FindConnectionContainer(GetDeviceId(), GetWorkstationId());

            if (connectionContainer == null)
                throw new Exception($"Cannot find remote device in the DeviceHub");

            return connectionContainer;
        }

        // HUB connection is connected
        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                string deviceId = httpContext.Request.Headers["DeviceId"].ToString();
                string workstationId = httpContext.Request.Headers["WorkstationId"].ToString();

                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    _logger.LogCritical($"DeviceId cannot be empty");
                }
                else if (string.IsNullOrWhiteSpace(workstationId))
                {
                    _logger.LogCritical($"WorkstationId cannot be empty");
                }
                else
                {
                    Context.Items.Add("DeviceId", deviceId);
                    Context.Items.Add("WorkstationId", workstationId);

                    _remoteDeviceConnectionsService.OnDeviceHubConnected(deviceId, workstationId, Clients.Caller);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DeviceHub.OnConnectedAsync error");
            }

            await base.OnConnectedAsync();
        }

        // HUB connection is disconnected (OnDeviceDisconnected received in AppHub)
        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                _remoteDeviceConnectionsService.OnDeviceHubDisconnected(GetDeviceId(), GetWorkstationId());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DeviceHub.OnDisconnectedAsync error");
            }
            return base.OnDisconnectedAsync(exception);
        }

        // Incoming request
        public HesResponse VerifyCommandResponse(DeviceCommandReplyResult deviceCommandReplyResult, string error)
        {
            try
            {
                var connectionContainer = GetDeviceConnectionContainer();
                connectionContainer.SetVerifyResponse(deviceCommandReplyResult, error);
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public HesResponse GetRootKeyCommandResponse(DeviceCommandReplyResult deviceCommandReplyResult, string error)
        {
            try
            {
                var connectionContainer = GetDeviceConnectionContainer();
                connectionContainer.SetGetRootKeyResponse(deviceCommandReplyResult, error);
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse(ex);
            }
        }

        // Incoming request
        public HesResponse RemoteCommandResponse(MessageBufferDto bufferDto, string error)
        {
            try
            {
                var connectionContainer = GetDeviceConnectionContainer();
                if (connectionContainer != null)
                {
                    MessageBuffer messageBuffer = new MessageBuffer(bufferDto.Data, bufferDto.ChannelNo);
                    connectionContainer.SetRemoteCommandResponse(messageBuffer, error);
                }
                return HesResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HesResponse(ex);
            }
        }
    }
}