using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.LicenseOrders;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class CreateLicenseOrder : OwningComponentBase
    {
        ILicenseService LicenseService { get; set; }
        IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<CreateLicenseOrder> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public ValidationErrorMessage ValidationErrorMessageNewOrder { get; set; }
        public ValidationErrorMessage ValidationErrorMessageRenewOrder { get; set; }
        public ButtonSpinner ButtonSpinnerNewOrder { get; set; }
        public ButtonSpinner ButtonSpinnerRenewOrder { get; set; }

        private NewLicenseOrder _newLicenseOrder;
        private RenewLicenseOrder _renewLicenseOrder;
        private bool _isBusy;
        private bool _initialized;

        protected override async Task OnInitializedAsync()
        {
            LicenseService = ScopedServices.GetRequiredService<ILicenseService>();
            HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();

            _newLicenseOrder = new NewLicenseOrder()
            {
                HardwareVaults = await HardwareVaultService.GetVaultsWithoutLicenseAsync()
            };

            _renewLicenseOrder = new RenewLicenseOrder()
            {
                HardwareVaults = await HardwareVaultService.GetVaultsWithLicenseAsync()
            };

            _initialized = true;
        }

        private async Task CreateNewLicenseOrderAsync()
        {
            try
            {
                await ButtonSpinnerNewOrder.SpinAsync(async () =>
                {
                    if (_newLicenseOrder.StartDate < DateTime.Now.Date)
                    {
                        ValidationErrorMessageNewOrder.DisplayError(nameof(NewLicenseOrder.StartDate), $"Start Date must be at least current date.");
                        return;
                    }

                    if (_newLicenseOrder.EndDate < _newLicenseOrder.StartDate)
                    {
                        ValidationErrorMessageNewOrder.DisplayError(nameof(NewLicenseOrder.EndDate), $"End Date must not be less than Start Date.");
                        return;
                    }

                    if (!_newLicenseOrder.HardwareVaults.Where(x => x.Checked).Any())
                    {
                        ValidationErrorMessageNewOrder.DisplayError(nameof(NewLicenseOrder.HardwareVaults), $"Select at least one hardware vault.");
                        return;
                    }

                    var licenseOrder = new LicenseOrder()
                    {
                        ContactEmail = _newLicenseOrder.ContactEmail,
                        Note = _newLicenseOrder.Note,
                        ProlongExistingLicenses = false,
                        StartDate = _newLicenseOrder.StartDate.Date,
                        EndDate = _newLicenseOrder.EndDate.Date
                    };

                    var checkedHardwareVaults = _newLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                    await LicenseService.CreateOrderAsync(licenseOrder, checkedHardwareVaults);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Licenses);
                    await ToastService.ShowToastAsync("Order created.", ToastType.Success);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task CreateRenewLicenseOrderAsync()
        {
            try
            {
                await ButtonSpinnerRenewOrder.SpinAsync(async () =>
                {
                    if (_renewLicenseOrder.EndDate < DateTime.Now)
                    {
                        ValidationErrorMessageRenewOrder.DisplayError(nameof(RenewLicenseOrder.EndDate), $"End Date must not be less than Start Date.");
                        return;
                    }

                    if (!_renewLicenseOrder.HardwareVaults.Where(x => x.Checked).Any())
                    {
                        ValidationErrorMessageRenewOrder.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), $"Select at least one hardware vault.");
                        return;
                    }

                    var checkedHardwareVaults = _renewLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                    var maxEndDate = checkedHardwareVaults.Select(x => x.LicenseEndDate).Max();

                    if (_renewLicenseOrder.EndDate < maxEndDate)
                    {
                        ValidationErrorMessageRenewOrder.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), $"The selected End Date less than max end date for selected hardware vaults.");
                        return;
                    }

                    var licenseOrder = new LicenseOrder()
                    {
                        ContactEmail = _renewLicenseOrder.ContactEmail,
                        Note = _renewLicenseOrder.Note,
                        ProlongExistingLicenses = true,
                        StartDate = null,
                        EndDate = _renewLicenseOrder.EndDate.Date
                    };

                    await LicenseService.CreateOrderAsync(licenseOrder, checkedHardwareVaults);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Licenses);
                    await ToastService.ShowToastAsync("Order created.", ToastType.Success);
                    await ModalDialogService.CloseAsync();
                });
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