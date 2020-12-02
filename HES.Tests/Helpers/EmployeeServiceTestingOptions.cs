using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using System.Collections.Generic;
using System.ComponentModel;

namespace HES.Tests.Helpers
{
    public class EmployeeServiceTestingOptions
    {
        public int AccountsCount { get; private set; }
        public int EmployeesCount { get; private set; }
        public string CrudEmployeeId { get; private set; }
        public Employee CrudEmployee { get; private set; }
        public string NewAccountName { get; private set; }
        public string AccountsEmployeeId { get; private set; }
        public Employee AccountsEmployee { get; private set; }
        public AccountAddModel PersonalAccount { get; private set; }
        public List<Employee> TestingEmployees { get; private set; }
        public DataLoadingOptions<EmployeeFilter> DataLoadingOptions { get; private set; }

        public EmployeeServiceTestingOptions(int employeeCount, int crudEmployeeId, int accountsEmployeeId)
        {
            EmployeesCount = employeeCount;
            CrudEmployeeId = $"{crudEmployeeId}";
            AccountsEmployeeId = $"{accountsEmployeeId}";

            NewAccountName = "New name";

            TestingEmployees = GetTestEmployees();

            DataLoadingOptions = new DataLoadingOptions<EmployeeFilter>()
            {
                Skip = 0,
                Take = EmployeesCount,
                SortDirection = ListSortDirection.Ascending,
                SortedColumn = nameof(Employee.Id),
                SearchText = string.Empty,
                Filter = null
            };

            PersonalAccount = new AccountAddModel()
            {
                Name = "stackoverflow",
                Urls = "stackoverflow.com",
                Login = "login1",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = AccountsEmployeeId
            };

            //WorkstationAccount = new WorkstationAccount()
            //{
            //    Name = "local",
            //    UserName = "user_local",
            //    Password = "qwerty",
            //    ConfirmPassword = "qwerty",
            //    EmployeeId = AccountsEmployeeId,
            //    Type = WorkstationAccountType.Local
            //};

            //WorkstationDomainAccount = new WorkstationDomain()
            //{
            //    Name = "domain",
            //    UserName = "user_domain",
            //    Password = "qwerty",
            //    ConfirmPassword = "qwerty",
            //    EmployeeId = AccountsEmployeeId,
            //};

            //WorkstationMsAccount = new WorkstationAccount()
            //{
            //    Name = "ms",
            //    UserName = "user_ms",
            //    Password = "qwerty",
            //    ConfirmPassword = "qwerty",
            //    EmployeeId = AccountsEmployeeId,
            //    Type = WorkstationAccountType.Microsoft
            //};

            //WorkstationAzureAccount = new WorkstationAccount()
            //{
            //    Name = "azure",
            //    UserName = "user_azure",
            //    Password = "qwerty",
            //    ConfirmPassword = "qwerty",
            //    EmployeeId = AccountsEmployeeId,
            //    Type = WorkstationAccountType.AzureAD
            //};

            AccountsCount = 5;
        }

        private List<Employee> GetTestEmployees()
        {
            var employees = new List<Employee>();

            for (int i = 0; i < EmployeesCount; i++)
            {
                var employee = new Employee
                {
                    Id = $"{i}",
                    FirstName = $"Test{i}",
                    LastName = $"Hideez{i}",
                    Email = $"th.{i}@hideez.com",
                    PhoneNumber = $"380{i}"
                };

                employees.Add(employee);

                if (employee.Id == CrudEmployeeId)
                    CrudEmployee = employee;
                else if (employee.Id == AccountsEmployeeId)
                    AccountsEmployee = employee;
            }

            return employees;
        }
    }
}