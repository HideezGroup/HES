using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVaults;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class HardwareVaultAccessProfilePage : HESComponentBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IMainTableService<HardwareVaultProfile, HardwareVaultProfileFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<HardwareVaultAccessProfilePage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<HardwareVaultProfile, HardwareVaultProfileFilter>>();

                SynchronizationService.UpdateHardwareVaultProfilesPage += UpdateHardwareVaultProfilesPage;

                await MainTableService.InitializeAsync(HardwareVaultService.GetHardwareVaultProfilesAsync, HardwareVaultService.GetHardwareVaultProfileCountAsync, ModalDialogService, StateHasChanged, nameof(HardwareVaultProfile.Name), ListSortDirection.Ascending);
                await BreadcrumbsService.SetHardwareVaultProfiles();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateHardwareVaultProfilesPage(string exceptPageId, string userName)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });
        }

        private async Task CreateProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateAccessProfile));
                builder.AddAttribute(1, nameof(CreateAccessProfile.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create Profile", body, ModalDialogSize.Default);
        }

        private async Task EditProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditProfile));
                builder.AddAttribute(1, nameof(EditProfile.ExceptPageId), PageId);
                builder.AddAttribute(2, nameof(EditProfile.HardwareVaultProfileId), MainTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Profile", body, ModalDialogSize.Default);
        }

        private async Task DeleteProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteProfile));
                builder.AddAttribute(1, nameof(DeleteProfile.ExceptPageId), PageId);
                builder.AddAttribute(2, nameof(DeleteProfile.HardwareVaultProfileId), MainTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Profile", body, ModalDialogSize.Default);
        }

        private async Task DetailsProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DetailsProfile));
                builder.AddAttribute(1, nameof(DetailsProfile.AccessProfile), MainTableService.SelectedEntity);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Details Profile", body, ModalDialogSize.Default);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateHardwareVaultProfilesPage -= UpdateHardwareVaultProfilesPage;
            MainTableService.Dispose();
        }
    }
}
