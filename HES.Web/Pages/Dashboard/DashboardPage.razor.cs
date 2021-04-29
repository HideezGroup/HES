using HES.Core.Interfaces;
using HES.Core.Models.Dashboard;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Dashboard
{
    public partial class DashboardPage : HESPageBase, IDisposable
    {
        public IDashboardService DashboardService { get; set; }
        [Inject] public ILogger<DashboardPage> Logger { get; set; }

        public DashboardCard ServerdCard { get; set; }
        public DashboardCard EmployeesCard { get; set; }
        public DashboardCard HardwareVaultsCard { get; set; }
        public DashboardCard WorkstationsCard { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                DashboardService = ScopedServices.GetRequiredService<IDashboardService>();

                await BreadcrumbsService.SetDashboard();
                ServerdCard = await DashboardService.GetServerCardAsync();
                ServerdCard.RightAction = ShowHardwareVaultTaskAsync;
                if (ServerdCard.Notifications.FirstOrDefault(x => x.Page == "long-pending-tasks") != null)
                    ServerdCard.Notifications.FirstOrDefault(x => x.Page == "long-pending-tasks").Action = ShowHardwareVaultTaskAsync;
                EmployeesCard = await DashboardService.GetEmployeesCardAsync();
                HardwareVaultsCard = await DashboardService.GetHardwareVaultsCardAsync();
                WorkstationsCard = await DashboardService.GetWorkstationsCardAsync();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task ShowHardwareVaultTaskAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(HardwareVaultTasks));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Hardware Vault Tasks", body, ModalDialogSize.Large);
        }

        public void Dispose()
        {

        }
    }
}
