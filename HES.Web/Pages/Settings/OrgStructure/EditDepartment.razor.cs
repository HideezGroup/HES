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
    public partial class EditDepartment : HESModalBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditDepartment> Logger { get; set; }
        [Parameter] public string DepartmentId { get; set; }

        public Department Department { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Department = await OrgStructureService.GetDepartmentByIdAsync(DepartmentId);
                if (Department == null)
                    throw new HESException(HESCode.DepartmentNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(Department.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Department.Id, Department);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        protected override async Task ModalDialogCancel()
        {
            OrgStructureService.UnchangedDepartment(Department);
            await base.ModalDialogCancel();
        }

        private async Task EditAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await OrgStructureService.EditDepartmentAsync(Department);
                    await ToastService.ShowToastAsync(Resources.Resource.OrgStructure_EditDepartment_Toast, ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (HESException ex) when (ex.Code == HESCode.DepartmentNameAlreadyInUse)
            {
                ValidationErrorMessage.DisplayError(nameof(Department.Name), ex.Message);
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
                MemoryCache.Remove(Department.Id);
        }
    }
}