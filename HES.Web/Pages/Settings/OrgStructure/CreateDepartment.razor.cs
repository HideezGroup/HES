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
    public partial class CreateDepartment : HESComponentBase
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreateDepartment> Logger { get; set; }
        [Parameter] public string CompanyId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Department Department { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            Department = new Department() { CompanyId = CompanyId };
        }

        private async Task CreateAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await OrgStructureService.CreateDepartmentAsync(Department);
                    await ToastService.ShowToastAsync("Department created.", ToastType.Success);
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
    }
}
