using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class DataTableTableHeaderItem : HESDomComponentBase
    {
        [Parameter] public string Title { get; set; }
        [Parameter] public double TitleWidth { get; set; } = 1;
        [Parameter] public string SortColumn { get; set; }
        [Parameter] public string CurrentSortedColumn { get; set; }
        [Parameter] public ListSortDirection CurrentSortDirection { get; set; }
        [Parameter] public Func<string, Task> SortedColumnChanged { get; set; }
        [Parameter] public Func<ListSortDirection, Task> SortDirectionChanged { get; set; }

        private async Task SortTable()
        {
            if (CurrentSortedColumn != SortColumn)
            {
                await SortedColumnChanged.Invoke(SortColumn);
            }
            else
            {
                CurrentSortDirection = CurrentSortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                await SortDirectionChanged.Invoke(CurrentSortDirection);
            }
        }

        private string GetSortIcon()
        {
            if (SortColumn != CurrentSortedColumn)
            {
                return string.Empty;
            }

            if (CurrentSortDirection == ListSortDirection.Ascending)
            {
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }
    }
}
