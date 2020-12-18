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
    public partial class EditPosition : HESComponentBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditPosition> Logger { get; set; }
        [Parameter] public string PositionId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Position Position { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ModalDialogService.OnCancel += ModalDialogService_OnCancel;

                Position = await OrgStructureService.GetPositionByIdAsync(PositionId);
                if (Position == null)
                    throw new Exception("Position not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(Position.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Position.Id, Position);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task EditAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await OrgStructureService.EditPositionAsync(Position);
                    await ToastService.ShowToastAsync("Position updated.", ToastType.Success);
                    await Refresh.InvokeAsync(this);
                    await SynchronizationService.UpdateOrgSructurePositions(ExceptPageId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Position.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task ModalDialogService_OnCancel()
        {
            await OrgStructureService.UnchangedPositionAsync(Position);
            ModalDialogService.OnCancel -= ModalDialogService_OnCancel;
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Position.Id);
        }
    }
}