using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SoftwareVault;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class SoftwareVaultsSection : ComponentBase
    {
        [Inject] public ISoftwareVaultService SoftwareVaultService { get; set; }

        public List<SoftwareVault> SoftwareVaults { get; set; }

        #region MainTable

        public SoftwareVaultFilter Filter { get; set; } = new SoftwareVaultFilter();
        public string SearchText { get; set; } = string.Empty;
        public string SortedColumn { get; set; } = nameof(SoftwareVault.OS);
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Ascending;
        public int DisplayRows { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalRecords { get; set; }
        public SoftwareVault SelectedSoftwareVault { get; set; }

        #endregion

        protected override async Task OnInitializedAsync()
        {
            //await LoadTableDataAsync();
        }

        #region MainTable

        //private async Task LoadTableDataAsync()
        //{
        //    var currentTotalRows = TotalRecords;
        //    TotalRecords = await SoftwareVaultService.GetVaultsCountAsync(SearchText, Filter);

        //    if (currentTotalRows != TotalRecords)
        //        CurrentPage = 1;

        //    SoftwareVaults = await SoftwareVaultService.GetSoftwareVaultsAsync((CurrentPage - 1) * DisplayRows, DisplayRows, SortedColumn, SortDirection, SearchText, Filter);
        //    SelectedSoftwareVault = null;

        //    StateHasChanged();
        //}

        //private async Task FilterChangedAsync(SoftwareVaultFilter filter)
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

        //private Task SelectedItemChangedAsync(SoftwareVault item)
        //{
        //    SelectedSoftwareVault = item;
        //    StateHasChanged();
        //    return Task.CompletedTask;
        //}

        //private Task SelectedItemDblClickAsync()
        //{
        //    return Task.CompletedTask;
        //}

        #endregion
    }
}