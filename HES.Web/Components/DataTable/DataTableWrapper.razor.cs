using Microsoft.AspNetCore.Components;

namespace HES.Web.Components
{
    public partial class DataTableWrapper : HESDomComponentBase
    {
        [Parameter] public RenderFragment WrapperHeader { get; set; }
        [Parameter] public RenderFragment WrapperBody { get; set; }
        [Parameter] public RenderFragment WrapperFooter { get; set; }
        [Parameter] public bool Loading { get; set; }
    }
}