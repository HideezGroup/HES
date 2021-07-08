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
    public partial class DeleteDepartment : HESModalBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteDepartment> Logger { get; set; }
        [Parameter] public string DepartmentId { get; set; }

        public Department Department { get; set; }
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

        public async Task DeleteAsync()
        {
            try
            {
                await OrgStructureService.DeleteDepartmentAsync(Department.Id);
                await ToastService.ShowToastAsync(Resources.Resource.OrgStructure_DeleteDepartment_Toast, ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
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