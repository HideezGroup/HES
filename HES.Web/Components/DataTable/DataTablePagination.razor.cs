using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class DataTablePagination : HESDomComponentBase
    {
        [Parameter] public Func<int, Task> CurrentPageChanged { get; set; }
        [Parameter] public Func<int, Task> DisplayRowsChanged { get; set; }
        [Parameter] public int ButtonRadius { get; set; } = 1;
        [Parameter] public int DisplayRows { get; set; } = 10;
        [Parameter] public int DisplayRecords { get; set; } = 10;
        [Parameter] public int CurrentPage { get; set; } = 1;
        [Parameter] public int TotalRecords { get; set; }
        [Parameter] public bool DisplayRecordsSelector { get; set; } = true;
        [Parameter] public bool DisplayTotalRecordsInfo { get; set; } = true;
        [Parameter] public string NextButton { get; set; } = "»";
        [Parameter] public string PrevButton { get; set; } = "«";

        public int TotalPages { get; set; }

        public List<PageLink> Links { get; set; }

        protected override void OnParametersSet()
        {
            TotalPages = (int)Math.Ceiling(TotalRecords / (decimal)DisplayRows);
            InitializePager();
        }

        public void InitializePager()
        {
            Links = new List<PageLink>();
            var isPrevButtonEnabled = CurrentPage != 1;

            var prevPage = CurrentPage - 1;
            Links.Add(new PageLink(prevPage, isPrevButtonEnabled, PrevButton));

            bool firstButton = false;


            for (int i = 1; i <= TotalPages; i++)
            {

                //Buttons from [1] to [7]
                if (TotalPages <= 7)
                {
                    Links.Add(new PageLink(i) { Active = CurrentPage == i });
                    continue;
                }

                //Buttons from [TotalPages - 3] to [Last]
                if (i >= TotalPages - 4 && CurrentPage >= TotalPages - 3)
                {
                    Links.Add(new PageLink(i) { Active = CurrentPage == i });
                    continue;
                }

                //Buttons from [1] to [5]
                if (i <= 5 && CurrentPage < 5)
                {
                    Links.Add(new PageLink(i) { Active = CurrentPage == i });
                    continue;
                }

                //Buttons [1][...]
                if (CurrentPage >= 5 && !firstButton)
                {
                    Links.Add(new PageLink(1, true));
                    Links.Add(new PageLink(0, false, "...") { Active = CurrentPage == 1 });
                    firstButton = true;
                    continue;
                }

                //Buttons radius
                if (CurrentPage <= TotalPages - 3 && (i >= CurrentPage - ButtonRadius && i <= CurrentPage + ButtonRadius))
                {
                    Links.Add(new PageLink(i) { Active = CurrentPage == i });
                    continue;
                }

                //Buttons [...][Last]
                if (i == TotalPages && CurrentPage <= TotalPages - 3)
                {
                    Links.Add(new PageLink(0, false, "..."));
                    Links.Add(new PageLink(TotalPages, true) { Active = CurrentPage == TotalPages });
                    continue;
                }
            }

            var isNextButtonEnabled = CurrentPage < TotalPages && TotalPages != 0;
            var nextPage = CurrentPage + 1;
            Links.Add(new PageLink(nextPage, isNextButtonEnabled, NextButton));
        }

        public async Task SelectedPageLinkAsync(PageLink pageLink)
        {
            if (pageLink.Page == CurrentPage)
            {
                return;
            }

            if (!pageLink.Enabled)
            {
                return;
            }

            CurrentPage = pageLink.Page;
            await CurrentPageChanged.Invoke(CurrentPage);
        }

        public async Task OnChangeShowEntries(ChangeEventArgs args)
        {
            DisplayRows = Convert.ToInt32(args.Value);
            await DisplayRowsChanged.Invoke(DisplayRows);
        }

        private int GetShowing()
        {
            if (DisplayRecords == DisplayRows || DisplayRecords > 0)
            {
                return (CurrentPage * DisplayRows - DisplayRows + 1);
            }
            else
            {
                return TotalPages;
            }
        }

        private int GetShowingTo()
        {
            if (CurrentPage <= TotalPages && TotalPages > 0)
            {
                if (CurrentPage == TotalPages)
                {
                    return TotalRecords;
                }
                else
                {
                    return (CurrentPage * DisplayRows);
                }
            }
            else
            {
                return TotalPages;
            }
        }
    }

    public class PageLink
    {
        public PageLink(int page) : this(page, true) { }
        public PageLink(int page, bool enabled) : this(page, enabled, page.ToString()) { }
        public PageLink(int page, bool enabled, string text)
        {
            Page = page;
            Enabled = enabled;
            Text = text;
        }

        public string Text { get; set; }
        public int Page { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Active { get; set; } = false;
    }
}