using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.LicenseOrders;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class CreateLicenseOrder : HESModalBase
    {
        ILicenseService LicenseService { get; set; }
        IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] ILogger<CreateLicenseOrder> Logger { get; set; }

        public ValidationErrorMessage ValidationErrorMessageNewOrder { get; set; }
        public ValidationErrorMessage ValidationErrorMessageRenewOrder { get; set; }
        public Button ButtonNewOrder { get; set; }
        public Button ButtonRenewOrder { get; set; }

        private NewLicenseOrder _newLicenseOrder;
        private RenewLicenseOrder _renewLicenseOrder;

        protected override async Task OnInitializedAsync()
        {
            try
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

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task CreateNewLicenseOrderAsync()
        {
            try
            {
                await ButtonNewOrder.SpinAsync(async () =>
                {
                    if (_newLicenseOrder.StartDate < DateTime.Now.Date)
                    {
                        ValidationErrorMessageNewOrder.DisplayError(nameof(NewLicenseOrder.StartDate), string.Format(Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_MustBeAtLeast, Resources.Resource.Label_StartDate));
                        return;
                    }

                    if (_newLicenseOrder.EndDate < _newLicenseOrder.StartDate)
                    {
                        ValidationErrorMessageNewOrder.DisplayError(nameof(NewLicenseOrder.EndDate), string.Format(Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_MustNotBeLess, Resources.Resource.Label_EndDate, Resources.Resource.Label_StartDate));
                        return;
                    }

                    if (!_newLicenseOrder.HardwareVaults.Where(x => x.Checked).Any())
                    {
                        ValidationErrorMessageNewOrder.DisplayError(nameof(NewLicenseOrder.HardwareVaults), Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_SelectAtLeastOne);
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
                    await ToastService.ShowToastAsync(Resources.Resource.LicenseOrders_CreateLicenseOrder_Toast, ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task CreateRenewLicenseOrderAsync()
        {
            try
            {
                await ButtonRenewOrder.SpinAsync(async () =>
                {
                    if (_renewLicenseOrder.EndDate < DateTime.Now)
                    {
                        ValidationErrorMessageRenewOrder.DisplayError(nameof(RenewLicenseOrder.EndDate), string.Format(Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_MustNotBeLess, Resources.Resource.Label_EndDate, Resources.Resource.Label_StartDate));
                        return;
                    }

                    if (!_renewLicenseOrder.HardwareVaults.Where(x => x.Checked).Any())
                    {
                        ValidationErrorMessageRenewOrder.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_SelectAtLeastOne);
                        return;
                    }

                    var checkedHardwareVaults = _renewLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                    var maxEndDate = checkedHardwareVaults.Select(x => x.LicenseEndDate).Max();

                    if (_renewLicenseOrder.EndDate < maxEndDate)
                    {
                        ValidationErrorMessageRenewOrder.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), string.Format(Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_LessThanMaxEndDate, Resources.Resource.Label_EndDate));
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
                    await ToastService.ShowToastAsync(Resources.Resource.LicenseOrders_CreateLicenseOrder_Toast, ToastType.Success);
                    await ModalDialogClose();
                });
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