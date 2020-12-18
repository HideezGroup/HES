using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class DeletePosition : HESComponentBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeletePosition> Logger { get; set; }
        [Parameter] public string PositionId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Position Position { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
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

        public async Task DeleteAsync()
        {
            try
            {
                await OrgStructureService.DeletePositionAsync(Position.Id);
                await Refresh.InvokeAsync(this); 
                await SynchronizationService.UpdateOrgSructurePositions(ExceptPageId);
                await ToastService.ShowToastAsync("Position removed.", ToastType.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                await ModalDialogService.CancelAsync();
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Position.Id);
        }
    }
}