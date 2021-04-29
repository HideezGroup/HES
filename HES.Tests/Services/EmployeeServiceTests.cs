using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Accounts;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using HES.Tests.Helpers;
using HES.Web;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
[assembly: TestCollectionOrderer("Xunit.Extensions.Ordering.CollectionOrderer", "Xunit.Extensions.Ordering")]
namespace HES.Tests.Services
{
    [Order(1)]
    public class EmployeeServiceTests : IClassFixture<CustomWebAppFactory<Startup>>
    {
        private readonly IEmployeeService _employeeService;
        private EmployeeServiceTestingOptions _testingOptions;

        public EmployeeServiceTests(CustomWebAppFactory<Startup> factory)
        {
            _employeeService = factory.GetEmployeeService();
            _testingOptions = new EmployeeServiceTestingOptions(100, 10, 20);
        }

        [Fact, Order(1)]
        public async Task CreateEmployeeAsync()
        {
            foreach (var employee in _testingOptions.TestingEmployees)
                await _employeeService.CreateEmployeeAsync(employee);

            //TODO GetEmployees 
            //var result = await _employeeService.EmployeeQuery().ToListAsync();

            //Assert.NotEmpty(result);
            //Assert.Equal(_testingOptions.EmployeesCount, result.Count);
        }

        [Fact, Order(2)]
        public async Task GetEmployeesCountAsync()
        {
            var employeesCount = await _employeeService.GetEmployeesCountAsync(_testingOptions.DataLoadingOptions);

            Assert.Equal(_testingOptions.EmployeesCount, employeesCount);
        }

        [Fact, Order(3)]
        public async Task GetEmployeesAsync()
        {
            var result = await _employeeService.GetEmployeesAsync(_testingOptions.DataLoadingOptions);

            Assert.NotEmpty(result);
            Assert.Equal(_testingOptions.EmployeesCount, result.Count);
        }

        [Fact, Order(4)]
        public async Task GetEmployeeByIdAsync()
        {
            var result = await _employeeService.GetEmployeeByIdAsync(_testingOptions.CrudEmployeeId);

            Assert.NotNull(result);
            Assert.Equal(_testingOptions.CrudEmployeeId, result.Id);
        }

        [Fact, Order(5)]
        public async Task CheckEmployeeNameExistAsync()
        {
            var result = await _employeeService.CheckEmployeeNameExistAsync(_testingOptions.CrudEmployee);
            var employeeResult = await _employeeService.GetEmployeeByIdAsync(_testingOptions.CrudEmployeeId);

            Assert.True(result);
            Assert.Equal(_testingOptions.CrudEmployee.FullName, employeeResult.FullName);
        }

        [Fact, Order(6)]
        public async Task EditEmployeeAsync()
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(_testingOptions.CrudEmployeeId);
            employee.FirstName = "NewFirstName";
            employee.LastName = "NewLastName";

            await _employeeService.EditEmployeeAsync(employee);
            var result = await _employeeService.GetEmployeeByIdAsync(_testingOptions.CrudEmployeeId);

            Assert.NotNull(result);
            Assert.Equal(employee, result);
        }

        [Fact, Order(7)]
        public async Task CreatePersonalAccountAsync()
        {
            var result = await _employeeService.CreatePersonalAccountAsync(_testingOptions.PersonalAccount);

            Assert.NotNull(result.Id);
            Assert.True(result.AccountType == AccountType.Personal);
        }

        [Fact, Order(8)]
        public async Task CreateWorkstationLocalAccountAsync()
        {
            var model = new AccountAddModel
            {
                Login = "WSLocalLogin",
                Name = "WSLocal",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = _testingOptions.AccountsEmployeeId,
                LoginType = LoginType.Local

            };
            var result = await _employeeService.CreatePersonalAccountAsync(model);

            Assert.NotNull(result.Id);
            Assert.True(result.AccountType == AccountType.Personal);
        }

        [Fact, Order(9)]
        public async Task CreateWorkstationDomainAccountAsync()
        {
            var model = new AccountAddModel
            {
                Login = "WSDomainLogin",
                Name = "WSDomain",
                Domain = "MyDomain",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = _testingOptions.AccountsEmployeeId,
                LoginType = LoginType.Domain
            };
            var result = await _employeeService.CreatePersonalAccountAsync(model);

            Assert.NotNull(result.Id);
            Assert.True(result.AccountType == AccountType.Personal);
            Assert.True(result.LoginType == LoginType.Domain);
        }

        [Fact, Order(10)]
        public async Task CreateWorkstationMSAccountAsync()
        {
            var model = new AccountAddModel
            {
                Login = "WSMicrosoftLogin",
                Name = "WSMicrosoft",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = _testingOptions.AccountsEmployeeId,
                LoginType = LoginType.Microsoft
            };
            var result = await _employeeService.CreatePersonalAccountAsync(model);

            Assert.NotNull(result.Id);
            Assert.True(result.AccountType == AccountType.Personal);
            Assert.True(result.LoginType == LoginType.Microsoft);
        }

        [Fact, Order(11)]
        public async Task CreateWorkstationAzureAccountAsync()
        {
            var model = new AccountAddModel
            {
                Login = "WSAzureADLogin",
                Name = "WSAzureAD",
                Password = "qwerty",
                ConfirmPassword = "qwerty",
                EmployeeId = _testingOptions.AccountsEmployeeId,
                LoginType = LoginType.AzureAD
            };
            var result = await _employeeService.CreatePersonalAccountAsync(model);

            Assert.NotNull(result.Id);
            Assert.True(result.AccountType == AccountType.Personal);
            Assert.True(result.LoginType == LoginType.AzureAD);
        }

        [Fact, Order(12)]
        public async Task GetAccountsCountAsync()
        {
            var options = new DataLoadingOptions<AccountFilter>() { EntityId = _testingOptions.AccountsEmployeeId };

            var accountsCount = await _employeeService.GetAccountsCountAsync(options);

            Assert.Equal(_testingOptions.AccountsCount, accountsCount);
        }

        [Fact, Order(13)]
        public async Task GetAccountsAsync()
        {
            var options = new DataLoadingOptions<AccountFilter>() { EntityId = _testingOptions.AccountsEmployeeId };

            var result = await _employeeService.GetAccountsAsync(options);

            Assert.NotEmpty(result);
            Assert.Equal(_testingOptions.AccountsCount, result.Count);
        }

        [Fact, Order(14)]
        public async Task GetAccountsByEmployeeIdAsync()
        {
            var result = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);

            Assert.NotNull(result);
            Assert.Equal(_testingOptions.AccountsCount, result.Count);
            Assert.All(result, x => Assert.True(x.EmployeeId == _testingOptions.AccountsEmployeeId));
        }

        [Fact, Order(15)]
        public async Task SetAsWorkstationAccountAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var account = accounts.FirstOrDefault(x => x.Name == "WSAzureAD");

            await _employeeService.SetAsPrimaryAccountAsync(_testingOptions.AccountsEmployeeId, account.Id);

            var employee = await _employeeService.GetEmployeeByIdAsync(_testingOptions.AccountsEmployeeId);

            Assert.True(employee.PrimaryAccountId == account.Id);
        }

        [Fact, Order(16)]
        public async Task GetAccountByIdAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var account = accounts.FirstOrDefault(x => x.Name == _testingOptions.PersonalAccount.Name);

            var result = await _employeeService.GetAccountByIdAsync(account.Id);

            Assert.NotNull(result);
            Assert.True(account.Id == result.Id);
            Assert.True(_testingOptions.AccountsEmployeeId == result.EmployeeId);
        }

        [Fact, Order(17)]
        public async Task UnchangedPersonalAccountAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var account = accounts.FirstOrDefault(x => x.Name == _testingOptions.PersonalAccount.Name);

            account.Name = "test";

            _employeeService.UnchangedPersonalAccount(account);

            Assert.True(account.Name == _testingOptions.PersonalAccount.Name);
        }

        [Fact, Order(18)]
        public async Task EditPersonalAccountAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var account = accounts.FirstOrDefault(x => x.Name == _testingOptions.PersonalAccount.Name);

            var personalAccount = new AccountEditModel().Initialize(account);

            personalAccount.Name = _testingOptions.NewAccountName;
            await _employeeService.EditPersonalAccountAsync(personalAccount);

            var accountsResult = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var result = accountsResult.FirstOrDefault(x => x.Name == _testingOptions.NewAccountName);

            Assert.NotNull(result);
            Assert.True(result.Id == account.Id);
            Assert.True(result.Name == _testingOptions.NewAccountName);
        }

        [Fact, Order(19)]
        public async Task EditPersonalAccountPwdAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var account = accounts.FirstOrDefault(x => x.Name == _testingOptions.NewAccountName);

            await _employeeService.EditPersonalAccountPwdAsync(account, new AccountPassword
            {
                Password = "newPassword",
                ConfirmPassword = "newPassword"
            });

            var accountsResult = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var result = accountsResult.FirstOrDefault(x => x.Name == _testingOptions.NewAccountName);

            Assert.NotNull(result);
            Assert.True(result.Id == account.Id);
            Assert.True(result.Password == "newPassword");
        }

        [Fact, Order(20)]
        public async Task EditPersonalAccountOtpAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var account = accounts.FirstOrDefault(x => x.Name == _testingOptions.NewAccountName);

            await _employeeService.EditPersonalAccountOtpAsync(account, new AccountOtp
            {
                OtpSecret = "newOtpSecret"
            });

            var accountsResult = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var result = accountsResult.FirstOrDefault(x => x.Name == _testingOptions.NewAccountName);

            Assert.NotNull(result);
            Assert.True(result.Id == account.Id);
            Assert.True(result.OtpSecret == "newOtpSecret");
        }

        [Fact, Order(21)]
        public async Task DeleteAccountAsync()
        {
            var accounts = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var account = accounts.FirstOrDefault(x => x.Name == _testingOptions.NewAccountName);

            await _employeeService.DeleteAccountAsync(account.Id);

            var accountsResult = await _employeeService.GetAccountsByEmployeeIdAsync(_testingOptions.AccountsEmployeeId);
            var result = accountsResult.FirstOrDefault(x => x.Id == account.Id);

            Assert.Null(result);
        }

        [Fact, Order(22)]
        public async Task DeleteEmployeeAsync()
        {
            await _employeeService.DeleteEmployeeAsync(_testingOptions.CrudEmployeeId);
            var result = await _employeeService.GetEmployeeByIdAsync(_testingOptions.CrudEmployeeId);

            Assert.Null(result);
        }

        // TODO
        // AddSharedAccountAsync
    }
}