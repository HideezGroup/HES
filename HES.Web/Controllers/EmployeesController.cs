using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.API.Employee;
using HES.Core.Models.Employees;
using HES.Core.Models.Accounts;
using HES.Core.Models.DataTableComponent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService employeeService,
                                   IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                                   ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _logger = logger;
        }

        #region Employee

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var count = await _employeeService.GetEmployeesCountAsync(new DataLoadingOptions<EmployeeFilter>());
            return await _employeeService.GetEmployeesAsync(new DataLoadingOptions<EmployeeFilter>
            {
                Take = count,
                SortedColumn = nameof(Employee.FullName),
                SortDirection = ListSortDirection.Ascending
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Employee>> GetEmployeeById(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Employee>>> GetFilteredEmployees(EmployeeFilter employeeFilter)
        {
            var count = await _employeeService.GetEmployeesCountAsync(new DataLoadingOptions<EmployeeFilter>
            {
                Filter = employeeFilter
            });

            return await _employeeService.GetEmployeesAsync(new DataLoadingOptions<EmployeeFilter>
            {
                Take = count,
                SortedColumn = nameof(Employee.FullName),
                SortDirection = ListSortDirection.Ascending,
                Filter = employeeFilter
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Employee>> CreateEmployee(CreateEmployeeDto employeeDto)
        {
            Employee createdEmployee;
            try
            {
                var employee = new Employee()
                {
                    FirstName = employeeDto.FirstName,
                    LastName = employeeDto.LastName,
                    Email = employeeDto.Email,
                    PhoneNumber = employeeDto.PhoneNumber,
                    DepartmentId = employeeDto.DepartmentId,
                    PositionId = employeeDto.PositionId
                };
                createdEmployee = await _employeeService.CreateEmployeeAsync(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetEmployeeById", new { id = createdEmployee.Id }, createdEmployee);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditEmployee(string id, EditEmployeeDto employeeDto)
        {
            if (id != employeeDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var employee = new Employee()
                {
                    Id = employeeDto.Id,
                    FirstName = employeeDto.FirstName,
                    LastName = employeeDto.LastName,
                    Email = employeeDto.Email,
                    PhoneNumber = employeeDto.PhoneNumber,
                    DepartmentId = employeeDto.DepartmentId,
                    PositionId = employeeDto.PositionId
                };
                await _employeeService.EditEmployeeAsync(employee);
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
        public async Task<ActionResult<Employee>> DeleteEmployee(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return employee;
        }

        #endregion

        #region Hardware Vault

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddHardwareVault(AddHardwareVaultDto vaultDto)
        {
            try
            {
                await _employeeService.AddHardwareVaultAsync(vaultDto.EmployeeId, vaultDto.HardwareVaultId);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(vaultDto.HardwareVaultId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveHardwareVault(RemoveHardwareVaultDto vaultDto)
        {
            try
            {
                await _employeeService.RemoveHardwareVaultAsync(vaultDto.HardwareVaultId, vaultDto.Reason, vaultDto.IsNeedBackup);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(vaultDto.HardwareVaultId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion

        #region Account

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccountsByEmployeeId(string id)
        {
            return await _employeeService.GetAccountsByEmployeeIdAsync(id);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Account>> GetAccountById(string id)
        {
            var account = await _employeeService.GetAccountByIdAsync(id);

            if (account == null)
            {
                _logger.LogWarning($"{nameof(account)} is null");
                return NotFound();
            }

            return account;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Account>> CreateAccount(CreateAccountDto accountDto)
        {
            Account createdAccount;
            try
            {
                var personalAccount = new AccountAddModel()
                {
                    Name = accountDto.Name,
                    Urls = accountDto.Urls,
                    Apps = accountDto.Apps,
                    LoginType = accountDto.LoginType,
                    Login = accountDto.Login,
                    EmployeeId = accountDto.EmployeeId,
                    Password = accountDto.Password,
                    OtpSecret = accountDto.OtpSecret
                };

                createdAccount = await _employeeService.CreatePersonalAccountAsync(personalAccount);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await _employeeService.GetEmployeeVaultIdsAsync(createdAccount.EmployeeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditAccount(string id, AccountEditModel accountDto)
        {
            if (id != accountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                await _employeeService.EditPersonalAccountAsync(accountDto);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await _employeeService.GetEmployeeVaultIdsAsync(accountDto.EmployeeId));
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
        public async Task<IActionResult> EditAccountPassword(string id, EditAccountPasswordDto accountDto)
        {
            if (id != accountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var account = await _employeeService.GetAccountByIdAsync(id);
                if (account == null)
                    return BadRequest("Account not found.");
                     
                var accountPassword = new AccountPassword()
                {
                    Password = accountDto.Password
                };

                await _employeeService.EditPersonalAccountPwdAsync(account, accountPassword);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await _employeeService.GetEmployeeVaultIdsAsync(account.EmployeeId));
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
        public async Task<IActionResult> EditAccountOtp(string id, EditAccountOtpDto accountDto)
        {
            if (id != accountDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var account = await _employeeService.GetAccountByIdAsync(id);
                if (account == null)
                    return BadRequest("Account not found.");

                var accountOtp = new AccountOtp()
                {
                    OtpSecret = accountDto.OtpSercret
                };

                await _employeeService.EditPersonalAccountOtpAsync(account, accountOtp);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await _employeeService.GetEmployeeVaultIdsAsync(account.EmployeeId));
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
        public async Task<ActionResult<Account>> DeleteAccount(string id)
        {
            Account account;
            try
            {
                account = await _employeeService.DeleteAccountAsync(id);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(account.Employee.HardwareVaults.Select(s => s.Id).ToList());
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return account;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddSharedAccount(AddSharedAccountDto sharedAccountDto)
        {
            Account createdAccount;
            try
            {
                createdAccount = await _employeeService.AddSharedAccountAsync(sharedAccountDto.EmployeeId, sharedAccountDto.SharedAccountId);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await _employeeService.GetEmployeeVaultIdsAsync(createdAccount.EmployeeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetAccountById", new { id = createdAccount.Id }, createdAccount);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SetAsWorkstationccount(SetAsWorkstationAccountDto accountDto)
        {
            try
            {
                await _employeeService.SetAsPrimaryAccountAsync(accountDto.EmployeeId, accountDto.AccountId);
                _remoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await _employeeService.GetEmployeeVaultIdsAsync(accountDto.EmployeeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        #endregion
    }
}