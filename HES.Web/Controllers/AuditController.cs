using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Audit;
using HES.Core.Models.Web.DataTableComponent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IWorkstationAuditService workstationAuditService, ILogger<AuditController> logger)
        {
            _workstationAuditService = workstationAuditService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationEvent>>> GetEvents()
        {
            var count = await _workstationAuditService.GetWorkstationEventsCountAsync(new DataLoadingOptions<WorkstationEventFilter>());
            return await _workstationAuditService.GetWorkstationEventsAsync(new DataLoadingOptions<WorkstationEventFilter>
            {
                Take = count,
                SortedColumn = nameof(WorkstationEvent.Date),
                SortDirection = ListSortDirection.Descending
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationEvent>>> GetFilteredEvents(WorkstationEventFilter workstationEventFilter)
        {
            var count = await _workstationAuditService.GetWorkstationEventsCountAsync(new DataLoadingOptions<WorkstationEventFilter>
            {
                Filter = workstationEventFilter
            });

            return await _workstationAuditService.GetWorkstationEventsAsync(new DataLoadingOptions<WorkstationEventFilter>
            {
                Take = count,
                SortedColumn = nameof(WorkstationEvent.Date),
                SortDirection = ListSortDirection.Descending,
                Filter = workstationEventFilter
            }); 
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationSession>>> GetSessions()
        {
            var count = await _workstationAuditService.GetWorkstationSessionsCountAsync(new DataLoadingOptions<WorkstationSessionFilter>());
            return await _workstationAuditService.GetWorkstationSessionsAsync(new DataLoadingOptions<WorkstationSessionFilter>
            {
                Take = count,
                SortedColumn = nameof(WorkstationSession.StartDate),
                SortDirection = ListSortDirection.Descending
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkstationSession>>> GetFilteredSessions(WorkstationSessionFilter workstationSessionFilter)
        {
            var count = await _workstationAuditService.GetWorkstationSessionsCountAsync(new DataLoadingOptions<WorkstationSessionFilter>
            {
                Filter = workstationSessionFilter
            });

            return await _workstationAuditService.GetWorkstationSessionsAsync(new DataLoadingOptions<WorkstationSessionFilter>
            {
                Take = count,
                SortedColumn = nameof(WorkstationSession.StartDate),
                SortDirection = ListSortDirection.Descending,
                Filter = workstationSessionFilter
            });
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByDayAndEmployee>>> GetSummaryByDayAndEmployees()
        {
            var count = await _workstationAuditService.GetSummaryByDayAndEmployeesCountAsync(new DataLoadingOptions<SummaryFilter>());
            return await _workstationAuditService.GetSummaryByDayAndEmployeesAsync(new DataLoadingOptions<SummaryFilter>
            {
                Take = count,
                SortedColumn = nameof(SummaryByDayAndEmployee.Date),
                SortDirection = ListSortDirection.Descending
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByDayAndEmployee>>> GetFilteredSummaryByDayAndEmployees(SummaryFilter summaryFilter)
        {
            var count = await _workstationAuditService.GetSummaryByDayAndEmployeesCountAsync(new DataLoadingOptions<SummaryFilter>
            {
                Filter = summaryFilter
            });

            return await _workstationAuditService.GetSummaryByDayAndEmployeesAsync(new DataLoadingOptions<SummaryFilter>
            {
                Take = count,
                SortedColumn = nameof(SummaryByDayAndEmployee.Date),
                SortDirection = ListSortDirection.Descending,
                Filter = summaryFilter
            });
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByEmployees>>> GetSummaryByEmployee()
        {
            var count = await _workstationAuditService.GetSummaryByEmployeesCountAsync(new DataLoadingOptions<SummaryFilter>());
            return await _workstationAuditService.GetSummaryByEmployeesAsync(new DataLoadingOptions<SummaryFilter>
            {
                Take = count,
                SortedColumn = nameof(SummaryByEmployees.Employee),
                SortDirection = ListSortDirection.Ascending
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByEmployees>>> GetFilteredSummaryByEmployees(SummaryFilter summaryFilter)
        {
            var count = await _workstationAuditService.GetSummaryByEmployeesCountAsync(new DataLoadingOptions<SummaryFilter>
            {
                Filter = summaryFilter
            });

            return await _workstationAuditService.GetSummaryByEmployeesAsync(new DataLoadingOptions<SummaryFilter>
            {
                Take = count,
                SortedColumn = nameof(SummaryByEmployees.Employee),
                SortDirection = ListSortDirection.Ascending,
                Filter = summaryFilter
            });
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByDepartments>>> GetSummaryByDepartments()
        {
            var count = await _workstationAuditService.GetSummaryByDepartmentsCountAsync(new DataLoadingOptions<SummaryFilter>());
            return await _workstationAuditService.GetSummaryByDepartmentsAsync(new DataLoadingOptions<SummaryFilter>
            {
                Take = count,
                SortedColumn = nameof(SummaryByDepartments.Company),
                SortDirection = ListSortDirection.Ascending
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByDepartments>>> GetFilteredSummaryByDepartments(SummaryFilter summaryFilter)
        {
            var count = await _workstationAuditService.GetSummaryByDepartmentsCountAsync(new DataLoadingOptions<SummaryFilter>
            {
                Filter = summaryFilter
            });

            return await _workstationAuditService.GetSummaryByDepartmentsAsync(new DataLoadingOptions<SummaryFilter>
            {
                Take = count,
                SortedColumn = nameof(SummaryByDepartments.Company),
                SortDirection = ListSortDirection.Ascending,
                Filter = summaryFilter
            });
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByWorkstations>>> GetSummaryByWorkstations()
        {
            var count = await _workstationAuditService.GetSummaryByWorkstationsCountAsync(new DataLoadingOptions<SummaryFilter>());
            return await _workstationAuditService.GetSummaryByWorkstationsAsync(new DataLoadingOptions<SummaryFilter>
            {
                Take = count,
                SortedColumn = nameof(SummaryByWorkstations.Workstation),
                SortDirection = ListSortDirection.Ascending
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SummaryByWorkstations>>> GetFilteredSummaryByWorkstations(SummaryFilter summaryFilter)
        {
            var count = await _workstationAuditService.GetSummaryByWorkstationsCountAsync(new DataLoadingOptions<SummaryFilter>
            {
                Filter = summaryFilter
            });

            return await _workstationAuditService.GetSummaryByWorkstationsAsync(new DataLoadingOptions<SummaryFilter>
            {
                Take = count,
                SortedColumn = nameof(SummaryByWorkstations.Workstation),
                SortDirection = ListSortDirection.Ascending,
                Filter = summaryFilter
            });
        }
    }
}