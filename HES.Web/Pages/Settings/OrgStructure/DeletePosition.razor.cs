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
    public partial class DeletePosition : HESModalBase, IDisposable
    {
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeletePosition> Logger { get; set; }
        [Parameter] public string PositionId { get; set; }

        public Position Position { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Position = await OrgStructureService.GetPositionByIdAsync(PositionId);
                if (Position == null)
                    throw new HESException(HESCode.PositionNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(Position.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Position.Id, Position);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public async Task DeleteAsync()
        {
            try
            {
                await OrgStructureService.DeletePositionAsync(Position.Id);
                await ToastService.ShowToastAsync(Resources.Resource.OrgStructure_DeletePosition_Toast, ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Position.Id);
        }
    }
}