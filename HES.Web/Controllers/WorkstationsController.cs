using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Workstations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WorkstationsController : ControllerBase
    {
        private readonly IWorkstationService _workstationService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<WorkstationsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkstationsController(
            IWorkstationService workstationService, 
            IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService, 
            ILogger<WorkstationsController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _workstationService = workstationService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
            _userManager = userManager;
        }

        #region Workstation

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Workstation>>> GetWorkstations()
        {
            var count = await _workstationService.GetWorkstationsCountAsync(new DataLoadingOptions<WorkstationFilter>());
            return await _workstationService.GetWorkstationsAsync(new DataLoadingOptions<WorkstationFilter>
            {
                Take = count,
                SortedColumn = nameof(Employee.FullName),
                SortDirection = ListSortDirection.Ascending
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Workstation>>> GetFilteredWorkstations(WorkstationFilter workstationFilter)
        {
            var count = await _workstationService.GetWorkstationsCountAsync(new DataLoadingOptions<WorkstationFilter>
            {
                Filter = workstationFilter
            });

            return await _workstationService.GetWorkstationsAsync(new DataLoadingOptions<WorkstationFilter>
            {
                Take = count,
                SortedColumn = nameof(Workstation.Name),
                SortDirection = ListSortDirection.Ascending,
                Filter = workstationFilter
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LockAllWorkstations()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                await _remoteWorkstationConnectionsService.LockAllWorkstationsAsync(user.Email);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Workstation>> GetWorkstationById(string id)
        {
            var workstation = await _workstationService.GetWorkstationByIdAsync(id);

            if (workstation == null)
            {
                return NotFound();
            }

            return workstation;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditWorkstation(string id, UpdateWorkstationDto workstationDto)
        {
            if (id != workstationDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var workstation = await _workstationService.GetWorkstationByIdAsync(workstationDto.Id);
                if (!workstation.Approved)
                {
                    return BadRequest(new { error = "Workstation not approved" });
                }

                workstation.DepartmentId = workstationDto.DepartmentId;
                workstation.RFID = workstationDto.RfidEnabled;

                await _workstationService.EditWorkstationAsync(workstation);
                await _remoteWorkstationConnectionsService.UpdateRfidStateAsync(workstation.Id, workstation.RFID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveWorkstation(string id, UpdateWorkstationDto workstationDto)
        {
            if (id != workstationDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var workstation = await _workstationService.GetWorkstationByIdAsync(workstationDto.Id);

                workstation.DepartmentId = workstationDto.DepartmentId;
                workstation.RFID = workstationDto.RfidEnabled;
                workstation.Approved = true;

                await _workstationService.ApproveWorkstationAsync(workstation);
                await _remoteWorkstationConnectionsService.UpdateRfidStateAsync(workstation.Id, workstation.RFID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnapproveWorkstation(string id)
        {
            try
            {
                await _workstationService.UnapproveWorkstationAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion

        #region Proximity

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationProximityVault>>> GetProximityVaults(string id)
        {
            var count = await _workstationService.GetProximityVaultsCountAsync(new DataLoadingOptions<WorkstationDetailsFilter>() { EntityId = id });
            return await _workstationService.GetProximityVaultsAsync(new DataLoadingOptions<WorkstationDetailsFilter>
            {
                Take = count,
                SortedColumn = nameof(Employee.FullName),
                SortDirection = ListSortDirection.Ascending,
                EntityId = id
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkstationProximityVault>> GetProximityVaultById(string id)
        {
            var proximityVault = await _workstationService.GetProximityVaultByIdAsync(id);

            if (proximityVault == null)
            {
                return NotFound();
            }

            return proximityVault;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddProximityVault(AddProximityVaultDto proximityVaultDto)
        {
            WorkstationProximityVault proximityDevice;
            try
            {
                proximityDevice = await _workstationService.AddProximityVaultAsync(proximityVaultDto.WorkstationId, proximityVaultDto.HardwareVaultId);
                await _remoteWorkstationConnectionsService.UpdateProximitySettingsAsync(proximityVaultDto.WorkstationId, await _workstationService.GetProximitySettingsAsync(proximityVaultDto.WorkstationId));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetProximityDeviceById", new { id = proximityDevice.Id }, proximityDevice);
        }

        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkstationProximityVault>> DeleteProximityDevice(string id)
        {
            var proximityDevice = await _workstationService.GetProximityVaultByIdAsync(id);
            if (proximityDevice == null)
            {
                return NotFound();
            }

            try
            {
                await _workstationService.DeleteProximityVaultAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return proximityDevice;
        }

        #endregion
    }
}
