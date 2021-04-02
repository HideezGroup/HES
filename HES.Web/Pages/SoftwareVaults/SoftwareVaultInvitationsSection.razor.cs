using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Core.Models.SoftwareVault;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class SoftwareVaultInvitationsSection : OwningComponentBase, IDisposable
    {
        public ISoftwareVaultService SoftwareVaultService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<SoftwareVaultInvitationsSection> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }

        public List<SoftwareVaultInvitation> SoftwareVaultInvitations { get; set; }
        public ServerSettings ServerSettings { get; set; }

        #region MainTable

        public SoftwareVaultInvitationFilter Filter { get; set; } = new SoftwareVaultInvitationFilter();
        public string SearchText { get; set; } = string.Empty;
        public string SortedColumn { get; set; } = nameof(SoftwareVaultInvitation.Id);
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
        public int DisplayRows { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalRecords { get; set; }
        public SoftwareVaultInvitation SelectedInvitation { get; set; }

        #endregion

        protected override async Task OnInitializedAsync()
        {
            SoftwareVaultService = ScopedServices.GetRequiredService<ISoftwareVaultService>();
            //await LoadTableDataAsync();   
        }

        //private async Task OpenDialogResendInvitationAsync()
        //{            
        //    RenderFragment body = (builder) =>
        //    {
        //        builder.OpenComponent(0, typeof(ResendSoftwareVaultInvitation));
        //        builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
        //        builder.AddAttribute(2, "SoftwareVaultInvitation", SelectedInvitation);
        //        builder.CloseComponent();
        //    };

        //    await ModalDialogService.ShowAsync("Resend invitation", body);
        //}

        //private async Task OpenDialogDeleteInvitationAsync()
        //{
        //    RenderFragment body = (builder) =>
        //    {
        //        builder.OpenComponent(0, typeof(DeleteSoftwareVaultInvitation));
        //        builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadTableDataAsync));
        //        builder.AddAttribute(2, "SoftwareVaultInvitation", SelectedInvitation);
        //        builder.CloseComponent();
        //    };

        //    await ModalDialogService.ShowAsync("Delete invitation", body);
        //}

        private string ShowButtons()
        {
            if (SelectedInvitation == null || SelectedInvitation?.Status == SoftwareVaultInvitationStatus.Accepted)
            {
                return "d-none";
            }
            return string.Empty;
        }

        #region MainTable

        //private async Task LoadTableDataAsync()
        //{
        //    var currentTotalRows = TotalRecords;
        //    TotalRecords = await SoftwareVaultService.GetInvitationsCountAsync(SearchText, Filter);

        //    if (currentTotalRows != TotalRecords)
        //        CurrentPage = 1;

        //    SoftwareVaultInvitations = await SoftwareVaultService.GetSoftwareVaultInvitationsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
        //    SelectedInvitation = null;

        //    StateHasChanged();
        //}

        //private async Task FilterChangedAsync(SoftwareVaultInvitationFilter filter)
        //{
        //    Filter = filter;
        //    await LoadTableDataAsync();
        //}

        //private async Task SearchTextChangedAsync(string searchText)
        //{
        //    SearchText = searchText;
        //    await LoadTableDataAsync();
        //}

        //private async Task SortedColumnChangedAsync(string columnName)
        //{
        //    SortedColumn = columnName;
        //    await LoadTableDataAsync();
        //}

        //private async Task SortDirectionChangedAsync(ListSortDirection sortDirection)
        //{
        //    SortDirection = sortDirection;
        //    await LoadTableDataAsync();
        //}

        //private async Task CurrentPageChangedAsync(int currentPage)
        //{
        //    CurrentPage = currentPage;
        //    await LoadTableDataAsync();
        //}

        //private async Task DisplayRowsChangedAsync(int displayRows)
        //{
        //    DisplayRows = displayRows;
        //    CurrentPage = 1;
        //    await LoadTableDataAsync();
        //}

        //private Task SelectedItemChangedAsync(SoftwareVaultInvitation item)
        //{
        //    SelectedInvitation = item;
        //    StateHasChanged();
        //    return Task.CompletedTask;
        //}

        //private Task SelectedItemDblClickAsync()
        //{
        //    return Task.CompletedTask;
        //}

        #endregion

        public void Dispose()
        {
            SoftwareVaultService.Dispose();
        }
    }
}
