using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace HES.Web.Components
{
    public class HESDomComponentBase : ComponentBase
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();
    }
}