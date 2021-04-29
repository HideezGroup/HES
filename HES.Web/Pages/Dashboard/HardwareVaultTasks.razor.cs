using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Dashboard
{
    public partial class HardwareVaultTasks : HESModalBase
    {
        [Inject] public IHardwareVaultTaskService HardwareVaultTaskService { get; set; }
        [Inject] public ILogger<HardwareVaultTasks> Logger { get; set; }
        public List<HardwareVaultTask> HardwareVaultTaskList { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultTaskList = await HardwareVaultTaskService.GetHardwareVaultTasksNoTrackingAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}