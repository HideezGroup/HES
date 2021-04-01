using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public interface IDataTableService<TItem, TFilter> where TFilter : class, new()
    {
        DataLoadingOptions<TFilter> DataLoadingOptions { get; set; }
        public TItem SelectedEntity { get; set; }
        public List<TItem> Entities { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public bool Loading { get; }
        public bool ShowFilter { get; }
        Task InitializeAsync(Func<DataLoadingOptions<TFilter>, Task<List<TItem>>> getEntities, Func<DataLoadingOptions<TFilter>, Task<int>> getEntitiesCount, Action stateHasChanged, string sortedColumn, ListSortDirection sortDirection = ListSortDirection.Ascending, string syncPropName = "Id", string entityId = null);
        Task LoadTableDataAsync();
        Task SelectedItemChangedAsync(TItem item);
        Task SortedColumnChangedAsync(string columnName);
        Task SortDirectionChangedAsync(ListSortDirection sortDirection);
        Task CurrentPageChangedAsync(int currentPage);
        Task DisplayRowsChangedAsync(int displayRows);
        Task SearchTextChangedAsync(string searchText);
        Task FilterChangedAsync(TFilter filter);
        void SetFilter(TFilter filter);
    }
}