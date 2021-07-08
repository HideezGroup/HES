using Microsoft.AspNetCore.Components;
using System;

namespace HES.Web.Components
{
    public partial class LanguageSelector : HESDomComponentBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        public string CurrentCulture { get; private set; } = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

        private void ChangeCulture(ChangeEventArgs args)
        {
            var culture = (string)args.Value;
            var uri = new Uri(NavigationManager.Uri).GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
            var query = $"?culture={Uri.EscapeDataString(culture)}&" + $"redirectUri={Uri.EscapeDataString(uri)}";
            NavigationManager.NavigateTo("/Culture/SetCulture" + query, true);
        }
    }
}