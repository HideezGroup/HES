using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class UnapproveWorkstation : HESModalBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<UnapproveWorkstation> Logger { get; set; }
        [Parameter] public string WorkstationId { get; set; }

        public Workstation Workstation { get; set; }
        public bool EntityBeingEdited { get; set; }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                RemoteWorkstationConnectionsService = ScopedServices.GetRequiredService<IRemoteWorkstationConnectionsService>();

                Workstation = await WorkstationService.GetWorkstationByIdAsync(WorkstationId);

                if (Workstation == null)
                    throw new HESException(HESCode.WorkstationNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(Workstation.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Workstation.Id, Workstation);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task UnapproveAsync()
        {
            try
            {
                await WorkstationService.UnapproveWorkstationAsync(Workstation.Id);
                await RemoteWorkstationConnectionsService.UpdateWorkstationApprovedAsync(Workstation.Id, isApproved: false);
                await ToastService.ShowToastAsync(Resources.Resource.Workstations_UnapproveWorkstation_Toast, ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Workstation.Id);
        }
    }
}