using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class CreateCompany : HESModalBase
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public ILogger<CreateCompany> Logger { get; set; }

        public Company Company { get; set; } = new Company();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }

        private async Task CreateAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await OrgStructureService.CreateCompanyAsync(Company);
                    await ToastService.ShowToastAsync(Resources.Resource.OrgStructure_CreateCompany_Toast, ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (HESException ex) when (ex.Code == HESCode.CompanyNameAlreadyInUse)
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
    }
}