using HES.Core.Enums;
using HES.Core.Models.Audit;
using HES.Web.Components;
using Hideez.SDK.Communication;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationEvents
{
    public partial class WorkstationEventsFilterComponent : ComponentBase
    {
        [Parameter] public Func<WorkstationEventFilter, Task> FilterChanged { get; set; }

        public WorkstationEventFilter Filter { get; set; }
        public List<string> EventTypes { get; set; }
        public List<string> EventSeverities { get; set; }
        public List<string> AccountTypes { get; set; }
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            EventTypes = Enum.GetValues(typeof(WorkstationEventType)).Cast<WorkstationEventType>().Select(s => s.ToString()).ToList();
            EventSeverities = Enum.GetValues(typeof(WorkstationEventSeverity)).Cast<WorkstationEventSeverity>().Select(s => s.ToString()).ToList();
            AccountTypes = Enum.GetValues(typeof(AccountType)).Cast<AccountType>().Select(s => s.ToString()).ToList();
            Filter = new WorkstationEventFilter();
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
            Filter = new WorkstationEventFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}