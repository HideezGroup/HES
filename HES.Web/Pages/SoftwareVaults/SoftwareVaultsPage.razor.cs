using HES.Core.Interfaces;
using HES.Core.Models.Web.Breadcrumb;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class SoftwareVaultsPage : ComponentBase
    {
        [Inject] public ILogger<SoftwareVaultsPage> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }

        public RenderFragment TabBody { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GetVaultsSectionAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var items = new List<Breadcrumb>()
                {
                    new Breadcrumb () { Active = true, Content = "Software Vaults" }
                };
                await JSRuntime.InvokeVoidAsync("createBreadcrumbs", items);
            }
        }

        private Task GetVaultsSectionAsync()
        {
            TabBody = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaultsSection));
                builder.CloseComponent();
            };

            return Task.CompletedTask;
        }

        private Task GetInvitationsSectionAsync()
        {
            TabBody = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaultInvitationsSection));
                builder.CloseComponent();
            };

            return Task.CompletedTask;
        }
    }
}
