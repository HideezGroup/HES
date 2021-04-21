using HES.Core.Enums;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Hideez.SDK.Communication;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public partial class WorkstationSessionsFilterComponent : ComponentBase
    {
        [Parameter] public Func<WorkstationSessionFilter, Task> FilterChanged { get; set; }

        public WorkstationSessionFilter Filter { get; set; }
        public List<string> SessionSwitchSubject { get; set; }
        public List<string> AccountTypes { get; set; }
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            SessionSwitchSubject = Enum.GetValues(typeof(SessionSwitchSubject)).Cast<SessionSwitchSubject>().Select(s => s.ToString()).ToList();
            AccountTypes = Enum.GetValues(typeof(AccountType)).Cast<AccountType>().Select(s => s.ToString()).ToList();
            Filter = new WorkstationSessionFilter();
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
            Filter = new WorkstationSessionFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}