using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Audit;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationAuditService : IWorkstationAuditService, IDisposable
    {
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;
        private readonly IAsyncRepository<Workstation> _workstationRepository;
        private readonly IAsyncRepository<HardwareVault> _hardwareVaultRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<Account> _accountRepository;
        private readonly IAsyncRepository<SummaryByDayAndEmployee> _summaryByDayAndEmployeeRepository;
        private readonly IAsyncRepository<SummaryByEmployees> _summaryByEmployeesRepository;
        private readonly IAsyncRepository<SummaryByDepartments> _summaryByDepartmentsRepository;
        private readonly IAsyncRepository<SummaryByWorkstations> _summaryByWorkstationsRepository;
        private readonly ILogger<WorkstationAuditService> _logger;

        public WorkstationAuditService(IAsyncRepository<WorkstationEvent> workstationEventRepository,
                                       IAsyncRepository<WorkstationSession> workstationSessionRepository,
                                       IAsyncRepository<Workstation> workstationRepository,
                                       IAsyncRepository<HardwareVault> hardwareVaultRepository,
                                       IAsyncRepository<Employee> employeeRepository,
                                       IAsyncRepository<Account> accountRepository,
                                       IAsyncRepository<SummaryByDayAndEmployee> summaryByDayAndEmployeeRepository,
                                       IAsyncRepository<SummaryByEmployees> summaryByEmployeesRepository,
                                       IAsyncRepository<SummaryByDepartments> summaryByDepartmentsRepository,
                                       IAsyncRepository<SummaryByWorkstations> summaryByWorkstationsRepository,
                                       ILogger<WorkstationAuditService> logger)
        {
            _workstationEventRepository = workstationEventRepository;
            _workstationSessionRepository = workstationSessionRepository;
            _workstationRepository = workstationRepository;
            _hardwareVaultRepository = hardwareVaultRepository;
            _employeeRepository = employeeRepository;
            _accountRepository = accountRepository;
            _summaryByDayAndEmployeeRepository = summaryByDayAndEmployeeRepository;
            _summaryByEmployeesRepository = summaryByEmployeesRepository;
            _summaryByDepartmentsRepository = summaryByDepartmentsRepository;
            _summaryByWorkstationsRepository = summaryByWorkstationsRepository;
            _logger = logger;
        }

        #region Event

        public IQueryable<WorkstationEvent> EventQuery()
        {
            return _workstationEventRepository.Query();
        }

        public async Task<List<WorkstationEvent>> GetWorkstationEventsAsync(DataLoadingOptions<WorkstationEventFilter> dataLoadingOptions)
        {
            var query = _workstationEventRepository
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.HardwareVault)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.Account)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.StartDate != null)
                {
                    query = query.Where(w => w.Date >= dataLoadingOptions.Filter.StartDate);
                }
                if (dataLoadingOptions.Filter.EndDate != null)
                {
                    query = query.Where(x => x.Date <= dataLoadingOptions.Filter.EndDate);
                }
                if (dataLoadingOptions.Filter.Event != null)
                {
                    query = query.Where(w => w.EventId == (WorkstationEventType)dataLoadingOptions.Filter.Event);
                }
                if (dataLoadingOptions.Filter.Severity != null)
                {
                    query = query.Where(w => w.SeverityId == (WorkstationEventSeverity)dataLoadingOptions.Filter.Severity);
                }
                if (dataLoadingOptions.Filter.Note != null)
                {
                    query = query.Where(w => w.Note.Contains(dataLoadingOptions.Filter.Note, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Workstation != null)
                {
                    query = query.Where(w => w.Workstation.Name.Contains(dataLoadingOptions.Filter.Workstation, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.UserSession != null)
                {
                    query = query.Where(w => w.UserSession.Contains(dataLoadingOptions.Filter.UserSession, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.HardwareVault != null)
                {
                    query = query.Where(w => w.HardwareVault.Id.Contains(dataLoadingOptions.Filter.HardwareVault, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(w => w.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(w => w.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Account != null)
                {
                    query = query.Where(w => w.Account.Name.Contains(dataLoadingOptions.Filter.Account, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.AccountType != null)
                {
                    query = query.Where(w => w.Account.AccountType == (AccountType)dataLoadingOptions.Filter.AccountType);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Note.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Workstation.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.UserSession.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVault.Id.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Account.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(WorkstationEvent.Date):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Date) : query.OrderByDescending(x => x.Date);
                    break;
                case nameof(WorkstationEvent.EventId):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.EventId) : query.OrderByDescending(x => x.EventId);
                    break;
                case nameof(WorkstationEvent.SeverityId):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.SeverityId) : query.OrderByDescending(x => x.SeverityId);
                    break;
                case nameof(WorkstationEvent.Note):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Note) : query.OrderByDescending(x => x.Note);
                    break;
                case nameof(WorkstationEvent.Workstation):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Workstation.Name) : query.OrderByDescending(x => x.Workstation.Name);
                    break;
                case nameof(WorkstationEvent.UserSession):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.UserSession) : query.OrderByDescending(x => x.UserSession);
                    break;
                case nameof(WorkstationEvent.HardwareVault):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVault.Id) : query.OrderByDescending(x => x.HardwareVault.Id);
                    break;
                case nameof(WorkstationEvent.Employee):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                    break;
                case nameof(WorkstationEvent.Department.Company):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Company.Name) : query.OrderByDescending(x => x.Department.Company.Name);
                    break;
                case nameof(WorkstationEvent.Department):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Name) : query.OrderByDescending(x => x.Department.Name);
                    break;
                case nameof(WorkstationEvent.Account):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Account.Name) : query.OrderByDescending(x => x.Account.Name);
                    break;
                case nameof(WorkstationEvent.Account.AccountType):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Account.AccountType) : query.OrderByDescending(x => x.Account.AccountType);
                    break;
            }

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetWorkstationEventsCountAsync(DataLoadingOptions<WorkstationEventFilter> dataLoadingOptions)
        {
            var query = _workstationEventRepository
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.HardwareVault)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.Account)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.StartDate != null)
                {
                    query = query.Where(w => w.Date >= dataLoadingOptions.Filter.StartDate);
                }
                if (dataLoadingOptions.Filter.EndDate != null)
                {
                    query = query.Where(x => x.Date <= dataLoadingOptions.Filter.EndDate);
                }
                if (dataLoadingOptions.Filter.Event != null)
                {
                    query = query.Where(w => w.EventId == (WorkstationEventType)dataLoadingOptions.Filter.Event);
                }
                if (dataLoadingOptions.Filter.Severity != null)
                {
                    query = query.Where(w => w.SeverityId == (WorkstationEventSeverity)dataLoadingOptions.Filter.Severity);
                }
                if (dataLoadingOptions.Filter.Note != null)
                {
                    query = query.Where(w => w.Note.Contains(dataLoadingOptions.Filter.Note, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Workstation != null)
                {
                    query = query.Where(w => w.Workstation.Name.Contains(dataLoadingOptions.Filter.Workstation, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.UserSession != null)
                {
                    query = query.Where(w => w.UserSession.Contains(dataLoadingOptions.Filter.UserSession, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.HardwareVault != null)
                {
                    query = query.Where(w => w.HardwareVault.Id.Contains(dataLoadingOptions.Filter.HardwareVault, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(w => w.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(w => w.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Account != null)
                {
                    query = query.Where(w => w.Account.Name.Contains(dataLoadingOptions.Filter.Account, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.AccountType != null)
                {
                    query = query.Where(w => w.Account.AccountType == (AccountType)dataLoadingOptions.Filter.AccountType);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Note.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Workstation.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.UserSession.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVault.Id.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Account.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task AddEventDtoAsync(WorkstationEventDto workstationEventDto)
        {
            if (workstationEventDto == null)
            {
                throw new ArgumentNullException(nameof(workstationEventDto));
            }

            var exist = await _workstationEventRepository.Query().AsNoTracking().AnyAsync(d => d.Id == workstationEventDto.Id);
            if (exist)
            {
                _logger.LogWarning($"[DUPLICATE EVENT][{workstationEventDto.WorkstationId}] EventId:{workstationEventDto.Id}, Session:{workstationEventDto.UserSession}, Event:{workstationEventDto.EventId}");
                return;
            }

            var workstation = await _workstationRepository.GetByIdAsync(workstationEventDto.WorkstationId);
            if (workstation == null)
            {
                throw new Exception($"Workstation not found, ID:{workstationEventDto.WorkstationId}");
            }

            Employee employee = null;
            Account account = null;

            if (workstationEventDto.DeviceId != null)
            {
                var hardwareVault = await _hardwareVaultRepository.GetByIdAsync(workstationEventDto.DeviceId);
                if (hardwareVault != null)
                {
                    employee = await _employeeRepository.GetByIdAsync(hardwareVault.EmployeeId);

                    account = await _accountRepository
                        .Query()
                        .Where(d => d.Name == workstationEventDto.AccountName &&
                                    d.Login == workstationEventDto.AccountLogin &&
                                    d.EmployeeId == hardwareVault.EmployeeId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }
            }

            var workstationEvent = new WorkstationEvent()
            {
                Id = workstationEventDto.Id,
                Date = workstationEventDto.Date,
                EventId = workstationEventDto.EventId,
                SeverityId = workstationEventDto.SeverityId,
                Note = workstationEventDto.Note,
                WorkstationId = workstationEventDto.WorkstationId,
                UserSession = workstationEventDto.UserSession,
                HardwareVaultId = workstationEventDto.DeviceId,
                EmployeeId = employee?.Id,
                DepartmentId = employee?.DepartmentId,
                AccountId = account?.Id,
            };

            await _workstationEventRepository.AddAsync(workstationEvent);
        }

        #endregion

        #region Session

        public IQueryable<WorkstationSession> SessionQuery()
        {
            return _workstationSessionRepository.Query();
        }

        public async Task<List<WorkstationSession>> GetWorkstationSessionsAsync(DataLoadingOptions<WorkstationSessionFilter> dataLoadingOptions)
        {
            var query = _workstationSessionRepository
                .Query()
                .Include(w => w.Workstation)
                .Include(w => w.HardwareVault)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.Account)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.StartDate != null)
                {
                    query = query.Where(w => w.StartDate >= dataLoadingOptions.Filter.StartDate);
                }
                if (dataLoadingOptions.Filter.EndDate != null)
                {
                    query = query.Where(x => x.EndDate <= dataLoadingOptions.Filter.EndDate);
                }
                if (dataLoadingOptions.Filter.UnlockedBy != null)
                {
                    query = query.Where(w => w.UnlockedBy == (SessionSwitchSubject)dataLoadingOptions.Filter.UnlockedBy);
                }
                if (dataLoadingOptions.Filter.Workstation != null)
                {
                    query = query.Where(w => w.Workstation.Name.Contains(dataLoadingOptions.Filter.Workstation, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.UserSession != null)
                {
                    query = query.Where(w => w.UserSession.Contains(dataLoadingOptions.Filter.UserSession, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.HardwareVault != null)
                {
                    query = query.Where(w => w.HardwareVault.Id.Contains(dataLoadingOptions.Filter.HardwareVault, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(w => w.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(w => w.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Account != null)
                {
                    query = query.Where(w => w.Account.Name.Contains(dataLoadingOptions.Filter.Account, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.AccountType != null)
                {
                    query = query.Where(w => w.Account.AccountType == (AccountType)dataLoadingOptions.Filter.AccountType);
                }
                if (dataLoadingOptions.Filter.Query != null)
                {
                    query = dataLoadingOptions.Filter.Query;
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Workstation.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.UserSession.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVault.Id.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Account.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(WorkstationSession.StartDate):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate);
                    break;
                case nameof(WorkstationSession.EndDate):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.EndDate) : query.OrderByDescending(x => x.EndDate);
                    break;
                case nameof(WorkstationSession.UnlockedBy):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.UnlockedBy) : query.OrderByDescending(x => x.UnlockedBy);
                    break;
                case nameof(WorkstationSession.Workstation):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Workstation.Name) : query.OrderByDescending(x => x.Workstation.Name);
                    break;
                case nameof(WorkstationSession.UserSession):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.UserSession) : query.OrderByDescending(x => x.UserSession);
                    break;
                case nameof(WorkstationSession.HardwareVault):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVault.Id) : query.OrderByDescending(x => x.HardwareVault.Id);
                    break;
                case nameof(WorkstationSession.Employee):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                    break;
                case nameof(WorkstationSession.Department.Company):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Company.Name) : query.OrderByDescending(x => x.Department.Company.Name);
                    break;
                case nameof(WorkstationSession.Department):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Name) : query.OrderByDescending(x => x.Department.Name);
                    break;
                case nameof(WorkstationSession.Account):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Account.Name) : query.OrderByDescending(x => x.Account.Name);
                    break;
                case nameof(WorkstationSession.Account.AccountType):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Account.AccountType) : query.OrderByDescending(x => x.Account.AccountType);
                    break;
            }

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetWorkstationSessionsCountAsync(DataLoadingOptions<WorkstationSessionFilter> dataLoadingOptions)
        {
            var query = _workstationSessionRepository
              .Query()
              .Include(w => w.Workstation)
              .Include(w => w.HardwareVault)
              .Include(w => w.Employee)
              .Include(w => w.Department.Company)
              .Include(w => w.Account)
              .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.StartDate != null)
                {
                    query = query.Where(w => w.StartDate >= dataLoadingOptions.Filter.StartDate);
                }
                if (dataLoadingOptions.Filter.EndDate != null)
                {
                    query = query.Where(x => x.EndDate <= dataLoadingOptions.Filter.EndDate);
                }
                if (dataLoadingOptions.Filter.UnlockedBy != null)
                {
                    query = query.Where(w => w.UnlockedBy == (SessionSwitchSubject)dataLoadingOptions.Filter.UnlockedBy);
                }
                if (dataLoadingOptions.Filter.Workstation != null)
                {
                    query = query.Where(w => w.Workstation.Name.Contains(dataLoadingOptions.Filter.Workstation, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.UserSession != null)
                {
                    query = query.Where(w => w.UserSession.Contains(dataLoadingOptions.Filter.UserSession, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.HardwareVault != null)
                {
                    query = query.Where(w => w.HardwareVault.Id.Contains(dataLoadingOptions.Filter.HardwareVault, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(w => w.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(w => w.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Account != null)
                {
                    query = query.Where(w => w.Account.Name.Contains(dataLoadingOptions.Filter.Account, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.AccountType != null)
                {
                    query = query.Where(w => w.Account.AccountType == (AccountType)dataLoadingOptions.Filter.AccountType);
                }
                if (dataLoadingOptions.Filter.Query != null)
                {
                    query = dataLoadingOptions.Filter.Query;
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Workstation.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.UserSession.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVault.Id.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Account.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task AddOrUpdateWorkstationSession(WorkstationEventDto workstationEventDto)
        {
            if (workstationEventDto == null)
            {
                throw new Exception(nameof(workstationEventDto));
            }

            // On connected or unlock
            if ((workstationEventDto.EventId == WorkstationEventType.HESConnected ||
                 workstationEventDto.EventId == WorkstationEventType.ServiceStarted ||
                 workstationEventDto.EventId == WorkstationEventType.ComputerUnlock ||
                 workstationEventDto.EventId == WorkstationEventType.ComputerLogon) &&
                 workstationEventDto.WorkstationSessionId != null)
            {
                var session = await _workstationSessionRepository.Query()
                    .FirstOrDefaultAsync(w => w.Id == workstationEventDto.WorkstationSessionId);

                if (session == null)
                {
                    // Add new session
                    await AddSessionAsync(workstationEventDto);
                }
                else
                {
                    // Reopen session
                    session.EndDate = null;
                    await UpdateSessionAsync(session);

                }
            }

            // On disconnected or lock
            if ((workstationEventDto.EventId == WorkstationEventType.ComputerLock ||
                 workstationEventDto.EventId == WorkstationEventType.ComputerLogoff) &&
                 workstationEventDto.WorkstationSessionId != null)
            {
                await CloseSessionAsync(workstationEventDto.WorkstationId);
            }
        }

        public async Task CloseSessionAsync(string workstationId)
        {
            var sessions = await _workstationSessionRepository
                .Query()
                .Where(w => w.WorkstationId == workstationId && w.EndDate == null)
                .ToListAsync();

            if (sessions != null)
            {
                foreach (var session in sessions)
                {
                    session.EndDate = DateTime.UtcNow;
                    await UpdateSessionAsync(session);
                }

                var sessionsCount = sessions.Count;
                if (sessionsCount > 1)
                {
                    _logger.LogError($"[{workstationId}] {sessionsCount} sessions were closed. id:{string.Join(". id:", sessions.Select(s => s.Id).ToArray())}");
                }
            }
        }

        private async Task AddSessionAsync(WorkstationEventDto workstationEventDto)
        {
            if (workstationEventDto == null)
            {
                throw new ArgumentNullException(nameof(workstationEventDto));
            }

            var workstation = await _workstationRepository.GetByIdAsync(workstationEventDto.WorkstationId);
            if (workstation == null)
            {
                throw new Exception($"Workstation not found, ID:{workstationEventDto.WorkstationId}");
            }

            Enum.TryParse(typeof(SessionSwitchSubject), workstationEventDto.Note, out object unlockMethod);
            SessionSwitchSubject unlockedBy = unlockMethod == null ? SessionSwitchSubject.NonHideez : (SessionSwitchSubject)unlockMethod;

            Employee employee = null;
            Account account = null;

            if (workstationEventDto.DeviceId != null)
            {
                var hardwareVault = await _hardwareVaultRepository.GetByIdAsync(workstationEventDto.DeviceId);
                if (hardwareVault != null)
                {
                    employee = await _employeeRepository.GetByIdAsync(hardwareVault.EmployeeId);

                    account = await _accountRepository
                        .Query()
                        .Where(d => d.Name == workstationEventDto.AccountName &&
                                    d.Login == workstationEventDto.AccountLogin &&
                                    d.EmployeeId == hardwareVault.EmployeeId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }
            }

            var workstationSession = new WorkstationSession()
            {
                Id = workstationEventDto.WorkstationSessionId,
                StartDate = workstationEventDto.Date,
                EndDate = null,
                UnlockedBy = unlockedBy,
                WorkstationId = workstationEventDto.WorkstationId,
                UserSession = workstationEventDto.UserSession,
                HardwareVaultId = workstationEventDto.DeviceId,
                EmployeeId = employee?.Id,
                DepartmentId = employee?.DepartmentId,
                AccountId = account?.Id,
            };

            await _workstationSessionRepository.AddAsync(workstationSession);
        }

        private async Task UpdateSessionAsync(WorkstationSession workstationSession)
        {
            if (workstationSession == null)
                throw new ArgumentNullException(nameof(workstationSession));

            await _workstationSessionRepository.UpdateOnlyPropAsync(workstationSession, new string[] { "EndDate" });
        }

        #endregion

        #region Summary

        public async Task<List<SummaryByDayAndEmployee>> GetSummaryByDayAndEmployeesAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();
            var orderby = string.Empty;

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.StartDate != null)
                {
                    filterParameters.Add($"Date >= '{dataLoadingOptions.Filter.StartDate.Value.ToString("yyyy-MM-dd")}'");
                }
                if (dataLoadingOptions.Filter.EndDate != null)
                {
                    filterParameters.Add($"Date <= '{dataLoadingOptions.Filter.EndDate.Value.ToString("yyyy-MM-dd")}'");
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    filterParameters.Add($"Employee LIKE '%{dataLoadingOptions.Filter.Employee}%'");
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Date LIKE '%{dataLoadingOptions.SearchText}%' OR Employee LIKE '%{dataLoadingOptions.SearchText}%' OR Company LIKE '%{dataLoadingOptions.SearchText}%' OR Department LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(SummaryByDayAndEmployee.Date):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Date ASC, Employee ASC" : "ORDER BY Date DESC, Employee ASC";
                    break;
                case nameof(SummaryByDayAndEmployee.Employee):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Employee ASC" : "ORDER BY Employee DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.Company):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Company ASC" : "ORDER BY Company DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.Department):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Department ASC" : "ORDER BY Department DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.WorkstationsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY WorkstationsCount ASC" : "ORDER BY WorkstationsCount DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.AvgSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgSessionsDuration ASC" : "ORDER BY AvgSessionsDuration DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.SessionsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY SessionsCount ASC" : "ORDER BY SessionsCount DESC";
                    break;
                case nameof(SummaryByDayAndEmployee.TotalSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY TotalSessionsDuration ASC" : "ORDER BY TotalSessionsDuration DESC";
                    break;
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }

            return await _summaryByDayAndEmployeeRepository.SqlQuery
                ($@"SELECT
	                    DATE(WorkstationSessions.StartDate) AS Date,
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                    GROUP BY
	                    DATE(WorkstationSessions.StartDate),
	                    WorkstationSessions.EmployeeId
                {having}
                {orderby}
                    LIMIT {dataLoadingOptions.Take} OFFSET {dataLoadingOptions.Skip}")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetSummaryByDayAndEmployeesCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.StartDate != null)
                {
                    filterParameters.Add($"Date >= '{dataLoadingOptions.Filter.StartDate.Value.ToString("yyyy-MM-dd")}'");
                }
                if (dataLoadingOptions.Filter.EndDate != null)
                {
                    filterParameters.Add($"Date <= '{dataLoadingOptions.Filter.EndDate.Value.ToString("yyyy-MM-dd")}'");
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    filterParameters.Add($"Employee LIKE '%{dataLoadingOptions.Filter.Employee}%'");
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Date LIKE '%{dataLoadingOptions.SearchText}%' OR Employee LIKE '%{dataLoadingOptions.SearchText}%' OR Company LIKE '%{dataLoadingOptions.SearchText}%' OR Department LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }

            return await _summaryByDayAndEmployeeRepository.SqlQuery
                ($@"SELECT
	                    DATE(WorkstationSessions.StartDate) AS Date,
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,
	                    COUNT(*) AS SessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                    GROUP BY
	                    DATE(WorkstationSessions.StartDate),
	                    WorkstationSessions.EmployeeId
                {having}
                    ORDER BY
	                    DATE(WorkstationSessions.StartDate) DESC, Employee ASC")
                .CountAsync();
        }

        public async Task<List<SummaryByEmployees>> GetSummaryByEmployeesAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();
            var orderby = string.Empty;

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    filterParameters.Add($"Employee LIKE '%{dataLoadingOptions.Filter.Employee}%'");
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Employee LIKE '%{dataLoadingOptions.SearchText}%' OR Company LIKE '%{dataLoadingOptions.SearchText}%' OR Department LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(SummaryByEmployees.Employee):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Employee ASC" : "ORDER BY Employee DESC";
                    break;
                case nameof(SummaryByEmployees.Company):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Company ASC" : "ORDER BY Company DESC";
                    break;
                case nameof(SummaryByEmployees.Department):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Department ASC" : "ORDER BY Department DESC";
                    break;
                case nameof(SummaryByEmployees.WorkstationsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY WorkstationsCount ASC" : "ORDER BY WorkstationsCount DESC";
                    break;
                case nameof(SummaryByEmployees.WorkingDaysCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY WorkingDaysCount ASC" : "ORDER BY WorkingDaysCount DESC";
                    break;
                case nameof(SummaryByEmployees.TotalSessionsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY TotalSessionsCount ASC" : "ORDER BY TotalSessionsCount DESC";
                    break;
                case nameof(SummaryByEmployees.TotalSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY TotalSessionsDuration ASC" : "ORDER BY TotalSessionsDuration DESC";
                    break;
                case nameof(SummaryByEmployees.AvgSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgSessionsDuration ASC" : "ORDER BY AvgSessionsDuration DESC";
                    break;
                case nameof(SummaryByEmployees.AvgSessionsCountPerDay):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgSessionsCountPerDay ASC" : "ORDER BY AvgSessionsCountPerDay DESC";
                    break;
                case nameof(SummaryByEmployees.AvgWorkingHoursPerDay):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgWorkingHoursPerDay ASC" : "ORDER BY AvgWorkingHoursPerDay DESC";
                    break;
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }

            return await _summaryByEmployeesRepository.SqlQuery
                 ($@"SELECT
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(DISTINCT DATE(WorkstationSessions.StartDate)) AS WorkingDaysCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    COUNT(*) / COUNT(DISTINCT DATE(WorkstationSessions.StartDate)) AS AvgSessionsCountPerDay,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT DATE(WorkstationSessions.StartDate))) AS AvgWorkingHoursPerDay
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id    
                    GROUP BY
	                    WorkstationSessions.EmployeeId
                 {having}
                 {orderby}
                    LIMIT {dataLoadingOptions.Take} OFFSET {dataLoadingOptions.Skip}")
                 .AsNoTracking()
                 .ToListAsync();
        }

        public async Task<int> GetSummaryByEmployeesCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    filterParameters.Add($"Employee LIKE '%{dataLoadingOptions.Filter.Employee}%'");
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Employee LIKE '%{dataLoadingOptions.SearchText}%' OR Company LIKE '%{dataLoadingOptions.SearchText}%' OR Department LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }

            return await _summaryByEmployeesRepository.SqlQuery
                 ($@"SELECT
	                    Employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(Employees.FirstName,' ',Employees.LastName), 'N/A') AS Employee,
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(DISTINCT DATE(WorkstationSessions.StartDate)) AS WorkingDaysCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    COUNT(*) / COUNT(DISTINCT DATE(WorkstationSessions.StartDate)) AS AvgSessionsCountPerDay,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT DATE(WorkstationSessions.StartDate))) AS AvgWorkingHoursPerDay
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id    
                    GROUP BY
	                    WorkstationSessions.EmployeeId
                  {having}")
                 .CountAsync();
        }

        public async Task<List<SummaryByDepartments>> GetSummaryByDepartmentsAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();
            var orderby = string.Empty;

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Company != null)
                {
                    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Company LIKE '%{dataLoadingOptions.SearchText}%' OR Department LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(SummaryByDepartments.Company):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Company ASC" : "ORDER BY Company DESC";
                    break;
                case nameof(SummaryByDepartments.Department):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Department ASC" : "ORDER BY Department DESC";
                    break;
                case nameof(SummaryByDepartments.EmployeesCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY EmployeesCount ASC" : "ORDER BY EmployeesCount DESC";
                    break;
                case nameof(SummaryByDepartments.WorkstationsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY WorkstationsCount ASC" : "ORDER BY WorkstationsCount DESC";
                    break;
                case nameof(SummaryByDepartments.TotalSessionsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY TotalSessionsCount ASC" : "ORDER BY TotalSessionsCount DESC";
                    break;
                case nameof(SummaryByDepartments.TotalSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY TotalSessionsDuration ASC" : "ORDER BY TotalSessionsDuration DESC";
                    break;
                case nameof(SummaryByDepartments.AvgSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgSessionsDuration ASC" : "ORDER BY AvgSessionsDuration DESC";
                    break;
                case nameof(SummaryByDepartments.AvgTotalDuartionByEmployee):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgTotalDuartionByEmployee ASC" : "ORDER BY AvgTotalDuartionByEmployee DESC";
                    break;
                case nameof(SummaryByDepartments.AvgTotalSessionsCountByEmployee):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgTotalSessionsCountByEmployee ASC" : "ORDER BY AvgTotalSessionsCountByEmployee DESC";
                    break;
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }
            return await _summaryByDepartmentsRepository.SqlQuery
                ($@"SELECT
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                    GROUP BY
	                    Departments.Id
                 {having}
                 {orderby}
                    LIMIT {dataLoadingOptions.Take} OFFSET {dataLoadingOptions.Skip}")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetSummaryByDepartmentsCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Company != null)
                {
                    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Company LIKE '%{dataLoadingOptions.SearchText}%' OR Department LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }
            return await _summaryByDepartmentsRepository.SqlQuery
                ($@"SELECT
	                    Companies.Id AS CompanyId,
	                    IFNULL(Companies.Name, 'N/A') AS Company,
	                    Departments.Id AS DepartmentId,
	                    IFNULL(Departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM WorkstationSessions
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id
                    GROUP BY
	                    Departments.Id
                 {having}")
                .CountAsync();
        }

        public async Task<List<SummaryByWorkstations>> GetSummaryByWorkstationsAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();
            var orderby = string.Empty;

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    filterParameters.Add($"Workstation LIKE '%{dataLoadingOptions.Filter.Workstation}%'");
                }
                //if (dataLoadingOptions.Filter.Company != null)
                //{
                //    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                //}
                //if (dataLoadingOptions.Filter.Department != null)
                //{
                //    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                //}
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Workstation LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(SummaryByWorkstations.Workstation):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Workstation ASC" : "ORDER BY Workstation DESC";
                    break;
                //case nameof(SummaryByWorkstations.Company):
                //    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Company ASC" : "ORDER BY Company DESC";
                //    break;
                //case nameof(SummaryByWorkstations.Department):
                //    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY Department ASC" : "ORDER BY Department DESC";
                //    break;
                case nameof(SummaryByWorkstations.EmployeesCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY EmployeesCount ASC" : "ORDER BY EmployeesCount DESC";
                    break;
                case nameof(SummaryByWorkstations.TotalSessionsCount):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY TotalSessionsCount ASC" : "ORDER BY TotalSessionsCount DESC";
                    break;
                case nameof(SummaryByWorkstations.TotalSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY TotalSessionsDuration ASC" : "ORDER BY TotalSessionsDuration DESC";
                    break;
                case nameof(SummaryByWorkstations.AvgSessionsDuration):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgSessionsDuration ASC" : "ORDER BY AvgSessionsDuration DESC";
                    break;
                case nameof(SummaryByWorkstations.AvgTotalDuartionByEmployee):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgTotalDuartionByEmployee ASC" : "ORDER BY AvgTotalDuartionByEmployee DESC";
                    break;
                case nameof(SummaryByWorkstations.AvgTotalSessionsCountByEmployee):
                    orderby = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? "ORDER BY AvgTotalSessionsCountByEmployee ASC" : "ORDER BY AvgTotalSessionsCountByEmployee DESC";
                    break;
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }

            return await _summaryByWorkstationsRepository.SqlQuery
                ($@"SELECT
	                    Workstations.Name AS Workstation,
	                    COUNT(DISTINCT IFNULL(Companies.Id, 'N/A')) AS CompaniesCount,
	                    COUNT(DISTINCT IFNULL(Departments.Id, 'N/A')) AS DepartmentsCount,
	                    COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM WorkstationSessions
	                    LEFT JOIN Workstations ON WorkstationSessions.WorkstationId = Workstations.Id
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id 
                    GROUP BY
	                    WorkstationId
                 {having}
                 {orderby}
                    LIMIT {dataLoadingOptions.Take} OFFSET {dataLoadingOptions.Skip}")
                 .AsNoTracking()
                 .ToListAsync();
        }

        public async Task<int> GetSummaryByWorkstationsCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions)
        {
            var having = string.Empty;
            var searchParameter = string.Empty;
            var filterParameters = new List<string>();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    filterParameters.Add($"Workstation LIKE '%{dataLoadingOptions.Filter.Workstation}%'");
                }
                //if (dataLoadingOptions.Filter.Company != null)
                //{
                //    filterParameters.Add($"Company LIKE '%{dataLoadingOptions.Filter.Company}%'");
                //}
                //if (dataLoadingOptions.Filter.Department != null)
                //{
                //    filterParameters.Add($"Department LIKE '%{dataLoadingOptions.Filter.Department}%'");
                //}
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();
                searchParameter = $"Workstation LIKE '%{dataLoadingOptions.SearchText}%'";
            }

            if (filterParameters.Count > 0 && searchParameter == string.Empty)
            {
                having = string.Join(" AND ", filterParameters).Insert(0, "HAVING ");
            }
            else if (filterParameters.Count == 0 && searchParameter != string.Empty)
            {
                having = $"HAVING {searchParameter}";
            }
            else if (filterParameters.Count > 0 && searchParameter != string.Empty)
            {
                var filter = string.Join(" AND ", filterParameters);
                having = $"HAVING ({filter}) AND ({searchParameter})";
            }

            return await _summaryByWorkstationsRepository.SqlQuery
                ($@"SELECT
	                    Workstations.Name AS Workstation,
	                    COUNT(DISTINCT IFNULL(Companies.Id, 'N/A')) AS CompaniesCount,
	                    COUNT(DISTINCT IFNULL(Departments.Id, 'N/A')) AS DepartmentsCount,
	                    COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate)))) AS AvgSessionsDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(WorkstationSessions.EndDate, NOW()), WorkstationSessions.StartDate))) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(Employees.Id, 'N/A')) AS AvgTotalSessionsCountByEmployee
                    FROM WorkstationSessions
	                    LEFT JOIN Workstations ON WorkstationSessions.WorkstationId = Workstations.Id
	                    LEFT JOIN Employees ON WorkstationSessions.EmployeeId = Employees.Id
	                    LEFT JOIN Departments ON Employees.DepartmentId = Departments.Id
	                    LEFT JOIN Companies ON Departments.CompanyId = Companies.Id 
                    GROUP BY
	                    WorkstationId
                 {having}")
                 .CountAsync();
        }


        #endregion

        public void Dispose()
        {
            _workstationEventRepository.Dispose();
            _workstationSessionRepository.Dispose();
            _workstationRepository.Dispose();
            _hardwareVaultRepository.Dispose();
            _employeeRepository.Dispose();
            _accountRepository.Dispose();
            _summaryByDayAndEmployeeRepository.Dispose();
            _summaryByEmployeesRepository.Dispose();
            _summaryByDepartmentsRepository.Dispose();
            _summaryByWorkstationsRepository.Dispose();
        }
    }
}