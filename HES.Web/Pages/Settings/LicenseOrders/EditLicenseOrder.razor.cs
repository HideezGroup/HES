using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.LicenseOrders;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class EditLicenseOrder : HESModalBase, IDisposable
    {
        public ILicenseService LicenseService { get; set; }
        public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditLicenseOrder> Logger { get; set; }
        [Parameter] public string LicenseOrderId { get; set; }
        public LicenseOrder LicenseOrder { get; set; }

        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }
        public bool EntityBeingEdited { get; set; }

        private NewLicenseOrder _newLicenseOrder;
        private RenewLicenseOrder _renewLicenseOrder;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LicenseService = ScopedServices.GetRequiredService<ILicenseService>();
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();

                LicenseOrder = await LicenseService.GetLicenseOrderByIdAsync(LicenseOrderId);
                if (LicenseOrder == null)
                    throw new HESException(HESCode.LicenseOrderNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(LicenseOrder.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(LicenseOrder.Id, LicenseOrder);

                if (!LicenseOrder.ProlongExistingLicenses)
                {
                    _newLicenseOrder = new NewLicenseOrder()
                    {
                        ContactEmail = LicenseOrder.ContactEmail,
                        Note = LicenseOrder.Note,
                        StartDate = LicenseOrder.StartDate.Value,
                        EndDate = LicenseOrder.EndDate,
                        HardwareVaults = await HardwareVaultService.GetVaultsWithoutLicenseAsync()
                    };
                    _newLicenseOrder.HardwareVaults.ForEach(x => x.Checked = LicenseOrder.HardwareVaultLicenses.Any(d => d.HardwareVaultId == x.Id));
                }
                else
                {
                    _renewLicenseOrder = new RenewLicenseOrder()
                    {
                        ContactEmail = LicenseOrder.ContactEmail,
                        Note = LicenseOrder.Note,
                        EndDate = LicenseOrder.EndDate,
                        HardwareVaults = await HardwareVaultService.GetVaultsWithLicenseAsync()
                    };
                    _renewLicenseOrder.HardwareVaults.ForEach(x => x.Checked = LicenseOrder.HardwareVaultLicenses.Any(d => d.HardwareVaultId == x.Id));
                }

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task EditNewLicenseOrderAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    if (_newLicenseOrder.StartDate < DateTime.Now.Date)
                    {
                        ValidationErrorMessage.DisplayError(nameof(NewLicenseOrder.StartDate), string.Format(Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_MustBeAtLeast, Resources.Resource.Label_StartDate));
                        return;
                    }

                    if (_newLicenseOrder.EndDate < _newLicenseOrder.StartDate)
                    {
                        ValidationErrorMessage.DisplayError(nameof(NewLicenseOrder.EndDate), string.Format(Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_MustNotBeLess, Resources.Resource.Label_EndDate, Resources.Resource.Label_StartDate));
                        return;
                    }

                    if (!_newLicenseOrder.HardwareVaults.Where(x => x.Checked).Any())
                    {
                        ValidationErrorMessage.DisplayError(nameof(NewLicenseOrder.HardwareVaults), Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_SelectAtLeastOne);
                        return;
                    }

                    LicenseOrder.ContactEmail = _newLicenseOrder.ContactEmail;
                    LicenseOrder.Note = _newLicenseOrder.Note;
                    LicenseOrder.ProlongExistingLicenses = false;
                    LicenseOrder.StartDate = _newLicenseOrder.StartDate.Date;
                    LicenseOrder.EndDate = _newLicenseOrder.EndDate.Date;

                    var checkedHardwareVaults = _newLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                    await LicenseService.EditOrderAsync(LicenseOrder, checkedHardwareVaults);
                    await ToastService.ShowToastAsync(Resources.Resource.LicenseOrders_EditLicenseOrder_Toast, ToastType.Success);
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

        private async Task EditRenewLicenseOrderAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    if (_renewLicenseOrder.EndDate < DateTime.Now)
                    {
                        ValidationErrorMessage.DisplayError(nameof(RenewLicenseOrder.EndDate), string.Format(Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_MustNotBeLess, Resources.Resource.Label_EndDate, Resources.Resource.Label_StartDate));
                        return;
                    }

                    if (!_renewLicenseOrder.HardwareVaults.Where(x => x.Checked).Any())
                    {
                        ValidationErrorMessage.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_SelectAtLeastOne);
                        return;
                    }

                    var checkedHardwareVaults = _renewLicenseOrder.HardwareVaults.Where(x => x.Checked).ToList();
                    var maxEndDate = checkedHardwareVaults.Select(x => x.LicenseEndDate).Max();

                    if (_renewLicenseOrder.EndDate < maxEndDate)
                    {
                        ValidationErrorMessage.DisplayError(nameof(RenewLicenseOrder.HardwareVaults), string.Format(Resources.Resource.LicenseOrders_CreateLicenseOrder_Error_LessThanMaxEndDate, Resources.Resource.Label_EndDate));
                        return;
                    }

                    LicenseOrder.ContactEmail = _renewLicenseOrder.ContactEmail;
                    LicenseOrder.Note = _renewLicenseOrder.Note;
                    LicenseOrder.ProlongExistingLicenses = true;
                    LicenseOrder.StartDate = null;
                    LicenseOrder.EndDate = _renewLicenseOrder.EndDate.Date;

                    await LicenseService.EditOrderAsync(LicenseOrder, checkedHardwareVaults);
                    await ToastService.ShowToastAsync(Resources.Resource.LicenseOrders_EditLicenseOrder_Toast, ToastType.Success);
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

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(LicenseOrder.Id);
        }
    }
}