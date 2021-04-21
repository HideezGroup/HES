using HES.Core.Entities;
using HES.Core.Models.Audit;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationAuditService
    {
        Task<List<WorkstationEvent>> GetWorkstationEventsAsync(DataLoadingOptions<WorkstationEventFilter> dataLoadingOptions);
        Task<int> GetWorkstationEventsCountAsync(DataLoadingOptions<WorkstationEventFilter> dataLoadingOptions);
        Task AddEventDtoAsync(WorkstationEventDto workstationEventDto);
        IQueryable<WorkstationSession> SessionQuery();
        Task<List<WorkstationSession>> GetWorkstationSessionsAsync(DataLoadingOptions<WorkstationSessionFilter> dataLoadingOptions);
        Task<int> GetWorkstationSessionsCountAsync(DataLoadingOptions<WorkstationSessionFilter> dataLoadingOptions);
        Task AddOrUpdateWorkstationSession(WorkstationEventDto workstationEventDto);
        Task CloseSessionAsync(string workstationId);
        Task<List<SummaryByDayAndEmployee>> GetSummaryByDayAndEmployeesAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<int> GetSummaryByDayAndEmployeesCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<List<SummaryByEmployees>> GetSummaryByEmployeesAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<int> GetSummaryByEmployeesCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<List<SummaryByDepartments>> GetSummaryByDepartmentsAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<int> GetSummaryByDepartmentsCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<List<SummaryByWorkstations>> GetSummaryByWorkstationsAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
        Task<int> GetSummaryByWorkstationsCountAsync(DataLoadingOptions<SummaryFilter> dataLoadingOptions);
    }
}