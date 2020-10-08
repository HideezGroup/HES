using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API.HardwareVault;
using HES.Core.Models.Web;
using HES.Core.Models.Web.HardwareVaults;
using HES.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.AdminRole)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HardwareVaultsController : ControllerBase
    {
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly ILogger<HardwareVaultsController> _logger;

        public HardwareVaultsController(IHardwareVaultService hardwareVaultService,
                                        IRemoteDeviceConnectionsService remoteDeviceConnectionsService,                                    
                                        ILogger<HardwareVaultsController> logger)
        {
            _hardwareVaultService = hardwareVaultService;
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _logger = logger;
        }

        #region Hardware Vault

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HardwareVault>>> GetHardwareVaults()
        {
            var count = await _hardwareVaultService.GetVaultsCountAsync(new DataLoadingOptions<HardwareVaultFilter>());
            return await _hardwareVaultService.GetVaultsAsync(new DataLoadingOptions<HardwareVaultFilter> 
            { 
                Take = count,
                SortedColumn = nameof(HardwareVault.Id),
                SortDirection = ListSortDirection.Ascending
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HardwareVault>>> GetFilteredHardwareVaults(HardwareVaultFilter hardwareVaultFilter)
        {
            var count = await _hardwareVaultService.GetVaultsCountAsync(new DataLoadingOptions<HardwareVaultFilter>
            {
                Filter = hardwareVaultFilter
            });

            return await _hardwareVaultService.GetVaultsAsync(new DataLoadingOptions<HardwareVaultFilter>
            {
                Take = count,
                SortedColumn = nameof(HardwareVault.Id),
                SortDirection = ListSortDirection.Ascending,
                Filter = hardwareVaultFilter
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HardwareVault>> GetHardwareVaultById(string id)
        {
            var vault = await _hardwareVaultService.GetVaultByIdAsync(id);

            if (vault == null)
                return NotFound();

            return vault;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditHardwareVaultRfid(string id, EditHardwareVaultRfidDto vaultDto)
        {
            if (id != vaultDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var vault = new HardwareVault()
                {
                    Id = vaultDto.Id,
                    RFID = vaultDto.RFID
                };

                await _hardwareVaultService.UpdateVaultAsync(vault);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SetAccessProfile(ChangeHardwareVaultProfileDto hardwareVaultProfileDto)
        {
            try
            {
                await _hardwareVaultService.ChangeVaultProfileAsync(hardwareVaultProfileDto.HardwareVaultId, hardwareVaultProfileDto.ProfileId);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(hardwareVaultProfileDto.HardwareVaultId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion

        #region Profile

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HardwareVaultProfile>>> GetProfiles()
        {
            return await _hardwareVaultService.GetProfilesAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HardwareVaultProfile>> GetProfileById(string id)
        {
            var profile = await _hardwareVaultService.GetProfileByIdAsync(id);

            if (profile == null)
                return NotFound();

            return profile;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<HardwareVaultProfile>> CreateProfile(CreateHardwareVaultProfileDto profileDto)
        {
            HardwareVaultProfile createdProfile;
            try
            {
                var deviceAccessProfile = new HardwareVaultProfile()
                {
                    Name = profileDto.Name,
                    ButtonPairing = profileDto.ButtonBonding,
                    ButtonConnection = profileDto.ButtonConnection,
                    ButtonStorageAccess = profileDto.ButtonNewChannel,
                    PinPairing = profileDto.PinBonding,
                    PinConnection = profileDto.PinConnection,
                    PinStorageAccess = profileDto.PinNewChannel,
                    MasterKeyConnection = profileDto.MasterKeyConnection,
                    MasterKeyStorageAccess = profileDto.MasterKeyNewChannel,
                    PinExpiration = profileDto.PinExpiration,
                    PinLength = profileDto.PinLength,
                    PinTryCount = profileDto.PinTryCount
                };
                createdProfile = await _hardwareVaultService.CreateProfileAsync(deviceAccessProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetProfileById", new { id = createdProfile.Id }, createdProfile);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditProfile(string id, EditHardwareVaultProfileDto profileDto)
        {
            if (id != profileDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var profile = new HardwareVaultProfile()
                {
                    Id = profileDto.Id,
                    Name = profileDto.Name,
                    ButtonPairing = profileDto.ButtonBonding,
                    ButtonConnection = profileDto.ButtonConnection,
                    ButtonStorageAccess = profileDto.ButtonNewChannel,
                    PinPairing = profileDto.PinBonding,
                    PinConnection = profileDto.PinConnection,
                    PinStorageAccess = profileDto.PinNewChannel,
                    MasterKeyConnection = profileDto.MasterKeyConnection,
                    MasterKeyStorageAccess = profileDto.MasterKeyNewChannel,
                    PinExpiration = profileDto.PinExpiration,
                    PinLength = profileDto.PinLength,
                    PinTryCount = profileDto.PinTryCount
                };
                await _hardwareVaultService.EditProfileAsync(profile);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await _hardwareVaultService.GetVaultIdsByProfileTaskAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HardwareVaultProfile>> DeleteProfile(string id)
        {
            var profile = await _hardwareVaultService.GetProfileByIdAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            try
            {
                await _hardwareVaultService.DeleteProfileAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return profile;
        }

        #endregion
    }
}
