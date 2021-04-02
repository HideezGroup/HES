using HES.Core.Models.DataTableComponent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class DataTableService<TItem, TFilter> : IDataTableService<TItem, TFilter> where TItem : class where TFilter : class, new()
    {
        private Func<DataLoadingOptions<TFilter>, Task<int>> _getEntitiesCount;
        private Func<DataLoadingOptions<TFilter>, Task<List<TItem>>> _getEntities;
        private Action _stateHasChanged;

        public DataLoadingOptions<TFilter> DataLoadingOptions { get; set; }
        public TItem SelectedEntity { get; set; }
        public List<TItem> Entities { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; } = 1;
        public string SyncPropName { get; set; }
        public bool Loading { get; private set; }
        public bool ShowFilter { get; private set; }

        public DataTableService()
        {
            DataLoadingOptions = new DataLoadingOptions<TFilter>();
        }

        public async Task InitializeAsync(Func<DataLoadingOptions<TFilter>,
            Task<List<TItem>>> getEntities, 
            Func<DataLoadingOptions<TFilter>, 
            Task<int>> getEntitiesCount,
            Action stateHasChanged,
            string sortedColumn,
            ListSortDirection sortDirection = ListSortDirection.Ascending,
            string syncPropName = "Id",
            string entityId = null)
        {
            _stateHasChanged = stateHasChanged;
            _getEntities = getEntities;
            _getEntitiesCount = getEntitiesCount;
            DataLoadingOptions.SortedColumn = sortedColumn;
            DataLoadingOptions.SortDirection = sortDirection;
            DataLoadingOptions.EntityId = entityId;
            SyncPropName = syncPropName;
            await LoadTableDataAsync();
        }

        public async Task LoadTableDataAsync()
        {
            if (Loading)
                return;

            try
            {
                Loading = true;
                _stateHasChanged?.Invoke();
                TotalRecords = await _getEntitiesCount.Invoke(DataLoadingOptions);
                SetCurrentPage();
                DataLoadingOptions.Skip = (CurrentPage - 1) * DataLoadingOptions.Take;
                Entities = await _getEntities.Invoke(DataLoadingOptions);
                SelectedEntity = Entities.FirstOrDefault(x => x.GetType().GetProperty(SyncPropName).GetValue(x).ToString() == SelectedEntity?.GetType().GetProperty(SyncPropName).GetValue(SelectedEntity).ToString());
            }
            finally
            {
                Loading = false;
                _stateHasChanged?.Invoke();
            }
        }

        public Task SelectedItemChangedAsync(TItem item)
        {
            SelectedEntity = item;
            _stateHasChanged?.Invoke();
            return Task.CompletedTask;
        }

        public async Task FilterChangedAsync(TFilter filter)
        {
            DataLoadingOptions.Filter = filter;
            await LoadTableDataAsync();
        }

        public async Task CurrentPageChangedAsync(int currentPage)
        {
            CurrentPage = currentPage;
            await LoadTableDataAsync();
        }

        public async Task DisplayRowsChangedAsync(int displayRows)
        {
            DataLoadingOptions.Take = displayRows;
            SetCurrentPage();
            await LoadTableDataAsync();
        }

        public async Task SearchTextChangedAsync(string searchText)
        {
            DataLoadingOptions.SearchText = searchText;
            await LoadTableDataAsync();
        }

        public async Task SortedColumnChangedAsync(string columnName)
        {
            DataLoadingOptions.SortedColumn = columnName;
            await LoadTableDataAsync();
        }

        public async Task SortDirectionChangedAsync(ListSortDirection sortDirection)
        {
            DataLoadingOptions.SortDirection = sortDirection;
            await LoadTableDataAsync();
        }

        private void SetCurrentPage()
        {
            var totalPages = (int)Math.Ceiling(TotalRecords / (decimal)DataLoadingOptions.Take);

            if (totalPages == 0)
            {
                CurrentPage = 1;
                return;
            }

            CurrentPage = totalPages < CurrentPage ? totalPages : CurrentPage;
        }
              
        public void SetFilter(TFilter filter)
        {
            DataLoadingOptions.Filter = filter;

            foreach (var item in filter.GetType().GetProperties())
            {
                var value = item.GetValue(filter);
                if (value != null)
                {
                    ShowFilter = true;
                    break;
                }
            }
        }
    }
}