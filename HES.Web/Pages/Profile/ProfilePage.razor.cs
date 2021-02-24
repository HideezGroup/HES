using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile
{
    public enum ProfileTabs
    {
        General,
        Security
    }

    public partial class ProfilePage : HESComponentBase
    {
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        public RenderFragment Tab { get; set; }
        public ProfileTabs SelectedTab { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SetGeneralTab();
            await BreadcrumbsService.SetProfile();
        }

        private void SetGeneralTab()
        {
            SelectedTab = ProfileTabs.General;
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(GeneralTab));
                builder.CloseComponent();
            };
        }

        private void SetSecurityTab()
        {
            SelectedTab = ProfileTabs.Security;
            Tab = (builder) =>
            {
                builder.OpenComponent(0, typeof(SecurityTab));
                builder.CloseComponent();
            };
        }

        private string SetActive(ProfileTabs tab)
        {
            return SelectedTab == tab ? "active" : string.Empty;
        }
    }
}