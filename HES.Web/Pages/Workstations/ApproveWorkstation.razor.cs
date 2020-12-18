using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class ApproveWorkstation : HESComponentBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IOrgStructureService OrgStructureService { get; set; }
        public IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<ApproveWorkstation> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string WorkstationId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public Workstation Workstation { get; set; }
        public List<Company> Companies { get; set; }
        public List<Department> Departments { get; set; }
        public bool EntityBeingEdited { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                OrgStructureService = ScopedServices.GetRequiredService<IOrgStructureService>();
                RemoteWorkstationConnectionsService = ScopedServices.GetRequiredService<IRemoteWorkstationConnectionsService>();

                Workstation = await WorkstationService.GetWorkstationByIdAsync(WorkstationId);

                if (Workstation == null)
                    throw new Exception("Workstation not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(Workstation.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Workstation.Id, Workstation);

                ModalDialogService.OnCancel += OnCancel;
                Companies = await OrgStructureService.GetCompaniesAsync();
                Departments = new List<Department>();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task ApproveAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await WorkstationService.ApproveWorkstationAsync(Workstation);
                    await RemoteWorkstationConnectionsService.UpdateRfidStateAsync(Workstation.Id, Workstation.RFID);
                    await RemoteWorkstationConnectionsService.UpdateWorkstationApprovedAsync(Workstation.Id, isApproved: true);
                    await ToastService.ShowToastAsync("Workstation approved.", ToastType.Success);
                    await SynchronizationService.UpdateWorkstations(ExceptPageId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task CompanyChangedAsync(ChangeEventArgs args)
        {
            Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(args.Value.ToString());
            Workstation.DepartmentId = Departments.FirstOrDefault()?.Id;
        }

        private async Task OnCancel()
        {
            await WorkstationService.UnchangedWorkstationAsync(Workstation);
            ModalDialogService.OnCancel -= OnCancel;
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Workstation.Id);
        }
    }
}