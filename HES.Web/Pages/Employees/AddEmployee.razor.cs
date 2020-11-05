using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddEmployee : OwningComponentBase
    {
        public ILdapService LdapService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddEmployee> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<ActiveDirectoryUser> Users { get; set; }
        public LdapSettings LdapSettings { get; set; }
        public ActiveDirectoryInitialization ActiveDirectoryInitialization { get; set; }
        public string WarningMessage { get; set; }
        public bool IsBusy { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public bool IsSortedAscending { get; set; } = true;
        public string CurrentSortColumn { get; set; } = nameof(Employee.FullName);
        public bool CreateAccounts { get; set; }
        public bool CreateGroups { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LdapService = ScopedServices.GetRequiredService<ILdapService>();

                LdapSettings = await AppSettingsService.GetLdapSettingsAsync();

                if (LdapSettings == null)
                {
                    ActiveDirectoryInitialization = ActiveDirectoryInitialization.HostNotSet;
                }
                else if (LdapSettings?.Host != null && LdapSettings?.UserName == null && LdapSettings?.Password == null)
                {
                    ActiveDirectoryInitialization = ActiveDirectoryInitialization.CredentialsNotSet;
                }
                else
                {
                    await GetUsers(LdapSettings);
                }

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task GetUsers(LdapSettings settings)
        {
            try
            {
                Users = await LdapService.GetUsersAsync(settings);
                ActiveDirectoryInitialization = ActiveDirectoryInitialization.Loaded;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task AddAsync()
        {
            try
            {
                if (!Users.Any(x => x.Checked))
                {
                    WarningMessage = "Please select at least one user.";
                    return;
                }

                await LdapService.AddUsersAsync(Users.Where(x => x.Checked).ToList(), CreateAccounts, CreateGroups);
                await ToastService.ShowToastAsync("Employee imported.", ToastType.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Employees);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private string GetSortIcon(string columnName)
        {
            if (CurrentSortColumn != columnName)
            {
                return string.Empty;
            }
            if (IsSortedAscending)
            {
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }

        private void SortTable(string columnName)
        {
            if (columnName != CurrentSortColumn)
            {
                Users = Users.OrderBy(x => x.Employee.GetType().GetProperty(columnName).GetValue(x.Employee, null)).ToList();
                CurrentSortColumn = columnName;
                IsSortedAscending = true;
            }
            else
            {
                if (IsSortedAscending)
                {
                    Users = Users.OrderByDescending(x => x.Employee.GetType().GetProperty(columnName).GetValue(x.Employee, null)).ToList();
                }
                else
                {
                    Users = Users.OrderBy(x => x.Employee.GetType().GetProperty(columnName).GetValue(x.Employee, null)).ToList();
                }

                IsSortedAscending = !IsSortedAscending;
            }
        }
    }
}