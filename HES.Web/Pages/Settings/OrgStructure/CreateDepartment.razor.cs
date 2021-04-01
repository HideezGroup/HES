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
    public partial class CreateDepartment : HESModalBase
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public ILogger<CreateDepartment> Logger { get; set; }
        [Parameter] public string CompanyId { get; set; }

        public Department Department { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            Department = new Department() { CompanyId = CompanyId };
        }

        private async Task CreateAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await OrgStructureService.CreateDepartmentAsync(Department);
                    await ToastService.ShowToastAsync("Department created.", ToastType.Success);
                    await ModalDialogClose();
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
                await ModalDialogCancel();
            }
        }
    }
}
