using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddSoftwareVault : ComponentBase
    {
        [Inject] public ISoftwareVaultService SoftwareVaultService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IOptions<ServerSettings> ServerSettings { get; set; }
        [Inject] public ILogger<AddSoftwareVault> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        //[Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public Employee Employee { get; set; } // TODO change to id
        [Parameter] public string ConnectionId { get; set; }

        public DateTime ValidTo { get; set; } = DateTime.Now;

        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            _initialized = true;
        }

        private async Task SendAsync()
        {
            //try
            //{
            //    await SoftwareVaultService.CreateAndSendInvitationAsync(Employee, ServerSettings.Value, ValidTo);
            //    await ToastService.ShowToastAsync("Invitation sent.", ToastType.Success);
            //    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, Employee.Id);
            //    await ModalDialogService.CloseAsync();
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogError(ex.Message);
            //    await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            //    await ModalDialogService.CloseAsync();
            //}
        }
    }
}
