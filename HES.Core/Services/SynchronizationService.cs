using HES.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public event Func<string, string, Task> UpdateEmployeePage;
        public event Func<string, string, string, Task> UpdateEmployeeDetailsPage;
        public event Func<string, Task> UpdateHardwareVaultState;

        public SynchronizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task UpdateEmployee(string exceptPageId)
        {
            if (UpdateEmployeePage != null)
            {
                var userName = _httpContextAccessor.HttpContext.User.Identity.Name;
                await UpdateEmployeePage.Invoke(exceptPageId, userName);
            }
        }

        public async Task UpdateEmployeeDetails(string exceptPageId, string employeeId)
        {
            if (UpdateEmployeeDetailsPage != null)
            {
                var userName = _httpContextAccessor.HttpContext.User.Identity.Name;
                await UpdateEmployeeDetailsPage.Invoke(exceptPageId, employeeId, userName);
            }
        }

        public async Task HardwareVaultStateChanged(string hardwareVaultId)
        {
            if (UpdateHardwareVaultState != null)
            {
                await UpdateHardwareVaultState.Invoke(hardwareVaultId);
            }
        }
    }
}
