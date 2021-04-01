using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class WorkstationSummaries : HESPageBase
    {
        public RenderFragment Tab { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await BreadcrumbsService.SetAuditSummaries();
            GetByDaysAndEmployeesTab();
        }

        private void GetByDaysAndEmployeesTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(ByDaysAndEmployeesTab));
                builder.CloseComponent();
            };
        }

        private void GetByEmployeesTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(ByEmployeesTab));
                builder.CloseComponent();
            };
        }

        private void GetByDepartmentsTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(ByDepartmentsTab));
                builder.CloseComponent();
            };
        }

        private void GetByWorkstationsTab()
        {
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(ByWorkstationsTab));
                builder.CloseComponent();
            };
        }
    }
}