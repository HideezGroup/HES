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
    public partial class EditCompany : HESModalBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditCompany> Logger { get; set; }
        [Parameter] public string CompanyId { get; set; }

        public Company Company { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Company = await OrgStructureService.GetCompanyByIdAsync(CompanyId);
                if (Company == null)
                    throw new Exception("Company not found.");

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

        protected override async Task ModalDialogCancel()
        {
            OrgStructureService.UnchangedCompany(Company);
            await base.ModalDialogCancel();
        }

        private async Task EditAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await OrgStructureService.EditCompanyAsync(Company);
                    await ToastService.ShowToastAsync("Company updated.", ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Company.Name), ex.Message);
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
                MemoryCache.Remove(Company.Id);
        }
    }
}