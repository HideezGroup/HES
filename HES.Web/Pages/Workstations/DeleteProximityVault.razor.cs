﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class DeleteProximityVault : HESModalBase
    {
        IWorkstationService WorkstationService { get; set; }
        [Inject] ILogger<DeleteProximityVault> Logger { get; set; }
        [Parameter] public WorkstationProximityVault WorkstationProximityVault { get; set; }
        [Parameter] public string WorkstationId { get; set; }

        protected override void OnInitialized()
        {
            WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
        }

        public async Task DeleteVaultAsync()
        {
            try
            {
                await WorkstationService.DeleteProximityVaultAsync(WorkstationProximityVault.Id);
                await ToastService.ShowToastAsync("Vault deleted.", ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}