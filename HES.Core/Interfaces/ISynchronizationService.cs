using System;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISynchronizationService
    {
        event Func<string, string, Task> UpdateEmployeePage;
        event Func<string, string, string, Task> UpdateEmployeeDetailsPage;
        event Func<string, Task> UpdateHardwareVaultState;

        Task UpdateEmployee(string exceptPageId);
        Task UpdateEmployeeDetails(string exceptPageId, string employeeId);

        Task HardwareVaultStateChanged(string hardwareVaultId);
    }
}