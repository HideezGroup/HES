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
    public partial class DeleteCompany : HESModalBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteCompany> Logger { get; set; }
        [Parameter] public string CompanyId { get; set; }

        public Company Company { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Company = await OrgStructureService.GetCompanyByIdAsync(CompanyId);
                if (Company == null)
                    throw new HESException(HESCode.CompanyNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(Company.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Company.Id, Company);

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
                await OrgStructureService.DeleteCompanyAsync(Company.Id);
                await ToastService.ShowToastAsync(Resources.Resource.OrgStructure_DeleteCompany_Toast, ToastType.Success);
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
                MemoryCache.Remove(Company.Id);
        }
    }
}