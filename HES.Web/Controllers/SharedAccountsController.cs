using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.API;
using HES.Core.Models.API.SharedAccount;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.SharedAccounts;
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
    public class SharedAccountsController : ControllerBase
    {
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly ILogger<SharedAccountsController> _logger;

        public SharedAccountsController(ISharedAccountService sharedAccountService,
                                        IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                                        ILogger<SharedAccountsController> logger)
        {
            _sharedAccountService = sharedAccountService;
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SharedAccount>>> GetSharedAccounts()
        {
            var count = await _sharedAccountService.GetSharedAccountsCountAsync(new DataLoadingOptions<SharedAccountsFilter>());
            return await _sharedAccountService.GetSharedAccountsAsync(new DataLoadingOptions<SharedAccountsFilter>
            {
                Take = count,
                SortedColumn = nameof(Employee.FullName),
                SortDirection = ListSortDirection.Ascending
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SharedAccount>> GetSharedAccountById(string id)
        {
            var sharedAccount = await _sharedAccountService.GetSharedAccountByIdAsync(id);

            if (sharedAccount == null)
            {
                return NotFound();
            }

            return sharedAccount;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedAccount(CreateSharedAccountDto sharedAccountDto)
        {
            SharedAccount createdAccount;
            try
            {
                var sharedAccount = new SharedAccount()
                {
                    Name = sharedAccountDto.Name,
                    Urls = sharedAccountDto.Urls,
                    Apps = sharedAccountDto.Apps,
                    Login = sharedAccountDto.Login,
                    Password = sharedAccountDto.Password,
                    OtpSecret = sharedAccountDto.OtpSecret
                };

                createdAccount = await _sharedAccountService.CreateSharedAccountAsync(sharedAccount);
            }
            catch (AlreadyExistException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedWorkstationLocalAccount(CreateWorkstationSharedAccountDto accountDto)
        {
            SharedAccount createdAccount;
            try
            {
                var workstationAccount = new WorkstationSharedAccount()
                {
                    Name = accountDto.Name,
                    UserName = accountDto.UserName,
                    Password = accountDto.Password,
                    Type = WorkstationAccountType.Local
                };

                createdAccount = await _sharedAccountService.CreateWorkstationSharedAccountAsync(workstationAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedWorkstationDomainAccount(CreateWorkstationDomainSharedAccountDto accountDto)
        {
            SharedAccount createdAccount;
            try
            {
                var workstationAccount = new WorkstationDomainSharedAccount()
                {
                    Name = accountDto.Name,
                    UserName = accountDto.UserName,
                    Domain = accountDto.Domain,
                    Password = accountDto.Password,
                    Type = WorkstationAccountType.Domain
                };

                createdAccount = await _sharedAccountService.CreateWorkstationSharedAccountAsync(workstationAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedWorkstationMicrosoftAccount(CreateWorkstationSharedAccountDto accountDto)
        {
            SharedAccount createdAccount;
            try
            {
                var workstationAccount = new WorkstationSharedAccount()
                {
                    Name = accountDto.Name,
                    UserName = accountDto.UserName,
                    Password = accountDto.Password,
                    Type = WorkstationAccountType.Microsoft
                };

                createdAccount = await _sharedAccountService.CreateWorkstationSharedAccountAsync(workstationAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SharedAccount>> CreateSharedWorkstationAzureAdAccount(CreateWorkstationSharedAccountDto accountDto)
        {
            SharedAccount createdAccount;
            try
            {
                var workstationAccount = new WorkstationSharedAccount()
                {
                    Name = accountDto.Name,
                    UserName = accountDto.UserName,
                    Password = accountDto.Password,
                    Type = WorkstationAccountType.AzureAD
                };

                createdAccount = await _sharedAccountService.CreateWorkstationSharedAccountAsync(workstationAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetSharedAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditSharedAccount(string id, EditSharedAccountDto sharedAccountDto)
        {
            if (id != sharedAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var sharedAccount = new SharedAccount()
                {
                    Id = sharedAccountDto.Id,
                    Name = sharedAccountDto.Name,
                    Urls = sharedAccountDto.Urls,
                    Apps = sharedAccountDto.Apps,
                    Login = sharedAccountDto.Login
                };

                var vaultIds = await _sharedAccountService.EditSharedAccountAsync(sharedAccount);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(vaultIds);
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
        public async Task<IActionResult> EditSharedAccountPassword(string id, EditSharedAccountPasswordDto sharedAccountDto)
        {
            if (id != sharedAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var sharedAccount = await _sharedAccountService.GetSharedAccountByIdAsync(id);
                var accountPassword = new AccountPassword()
                {
                    Password = sharedAccountDto.Password
                };

                var vaultIds = await _sharedAccountService.EditSharedAccountPwdAsync(sharedAccount, accountPassword);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(vaultIds);
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
        public async Task<IActionResult> EditSharedAccountOtp(string id, EditSharedAccountOtpDto sharedAccountDto)
        {
            if (id != sharedAccountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var sharedAccount = await _sharedAccountService.GetSharedAccountByIdAsync(id);
                var accountOtp = new AccountOtp()
                {
                    OtpSecret = sharedAccountDto.OtpSecret
                };

                var vaultIds = await _sharedAccountService.EditSharedAccountOtpAsync(sharedAccount, accountOtp);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(vaultIds);
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
        public async Task<ActionResult<SharedAccount>> DeleteSharedAccount(string id)
        {
            var sharedAccount = await _sharedAccountService.GetSharedAccountByIdAsync(id);
            if (sharedAccount == null)
            {
                return NotFound();
            }

            try
            {
                var vaultIds = await _sharedAccountService.DeleteSharedAccountAsync(id);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(vaultIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
            return sharedAccount;
        }
    }
}
