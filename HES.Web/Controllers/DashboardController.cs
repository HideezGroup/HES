using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public string GetServerVersion()
        {
            return _dashboardService.GetServerVersion();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetDeviceTasksCount()
        {
            return await _dashboardService.GetHardwareVaultTasksCount();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HardwareVaultTask>>> GetDeviceTasks()
        {
            return await _dashboardService.GetVaultTasks();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DashboardNotify>>> GetServerNotify()
        {
            return await _dashboardService.GetServerNotifyAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetEmployeesCount()
        {
            return await _dashboardService.GetEmployeesCountAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetEmployeesOpenedSessionsCount()
        {
            return await _dashboardService.GetEmployeesOpenedSessionsCountAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DashboardNotify>>> GetEmployeesNotify()
        {
            return await _dashboardService.GetEmployeesNotifyAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetDevicesCount()
        {
            return await _dashboardService.GetHardwareVaultsCountAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetFreeDevicesCount()
        {
            return await _dashboardService.GetReadyHardwareVaultsCountAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DashboardNotify>>> GetDevicesNotify()
        {
            return await _dashboardService.GetHardwareVaultsNotifyAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetWorkstationsCount()
        {
            return await _dashboardService.GetWorkstationsCountAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<int> GetWorkstationsOnlineCount()
        {
            return await _dashboardService.GetWorkstationsOnlineCountAsync();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DashboardNotify>>> GetWorkstationsNotify()
        {
            return await _dashboardService.GetWorkstationsNotifyAsync();
        }
    }
}