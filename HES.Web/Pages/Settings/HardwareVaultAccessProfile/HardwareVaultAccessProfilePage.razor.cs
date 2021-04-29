using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class HardwareVaultAccessProfilePage : HESPageBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IDataTableService<HardwareVaultProfile, HardwareVaultProfileFilter> DataTableService { get; set; }
        [Inject] public ILogger<HardwareVaultAccessProfilePage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<HardwareVaultProfile, HardwareVaultProfileFilter>>();

                PageSyncService.UpdateHardwareVaultProfilesPage += UpdateHardwareVaultProfilesPage;

                await DataTableService.InitializeAsync(HardwareVaultService.GetHardwareVaultProfilesAsync, HardwareVaultService.GetHardwareVaultProfileCountAsync, StateHasChanged, nameof(HardwareVaultProfile.Name), ListSortDirection.Ascending);
                await BreadcrumbsService.SetHardwareVaultProfiles();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateHardwareVaultProfilesPage(string exceptPageId)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });
        }

        private async Task CreateProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateAccessProfile));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Create Profile", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
            }
        }

        private async Task EditProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditProfile));
                builder.AddAttribute(2, nameof(EditProfile.HardwareVaultProfileId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Edit Profile", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
            }
        }

        private async Task DeleteProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteProfile));
                builder.AddAttribute(2, nameof(DeleteProfile.HardwareVaultProfileId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Delete Profile", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
            }
        }

        private async Task DetailsProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DetailsProfile));
                builder.AddAttribute(1, nameof(DetailsProfile.AccessProfile), DataTableService.SelectedEntity);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Details Profile", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateHardwareVaultProfilesPage -= UpdateHardwareVaultProfilesPage;
        }
    }
}