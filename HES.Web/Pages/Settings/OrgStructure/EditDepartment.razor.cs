using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class EditDepartment : HESComponentBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditDepartment> Logger { get; set; }
        [Parameter] public string DepartmentId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Department Department { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ModalDialogService.OnCancel += ModalDialogService_OnCancel;

                Department = await OrgStructureService.GetDepartmentByIdAsync(DepartmentId);
                if (Department == null)
                    throw new Exception("Department not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(Department.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Department.Id, Department);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task EditAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await OrgStructureService.EditDepartmentAsync(Department);
                    await ToastService.ShowToastAsync("Department updated.", ToastType.Success);
                    await Refresh.InvokeAsync(this);
                    await SynchronizationService.UpdateOrgSructureCompanies(ExceptPageId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Department.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task ModalDialogService_OnCancel()
        {
            await OrgStructureService.UnchangedDepartmentAsync(Department);
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= ModalDialogService_OnCancel;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Department.Id);
        }
    }
}