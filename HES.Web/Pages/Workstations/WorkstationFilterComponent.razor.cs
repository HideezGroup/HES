using HES.Core.Interfaces;
using HES.Core.Models.Workstations;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class WorkstationFilterComponent : ComponentBase
    {
        [Inject] public IWorkstationService WorkstationService { get; set; }
        [Parameter] public Func<WorkstationFilter, Task> FilterChanged { get; set; }

        public WorkstationFilter Filter { get; set; }
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            Filter = new WorkstationFilter();
        }

        private async Task FilteredAsync()
        {
            await Button.SpinAsync(async () =>
            {
                await FilterChanged.Invoke(Filter);
            });
        }

        private async Task ClearAsync()
        {
            Filter = new WorkstationFilter();
            await FilterChanged.Invoke(Filter);
        }

        private void OnChangeRfid(ChangeEventArgs args)
        {
            var value = (string)args.Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                Filter.RFID = null;
                return;
            }

            if (value == "Yes")
            {
                Filter.RFID = true;
            }
            else
            {
                Filter.RFID = false;
            }
        }

        private void OnChangeApproved(ChangeEventArgs args)
        {
            var value = (string)args.Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                Filter.Approved = null;
                return;
            }

            if (value == "Yes")
            {
                Filter.Approved = true;
            }
            else
            {
                Filter.Approved = false;
            }
        }
    }
}