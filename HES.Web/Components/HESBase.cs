using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Components
{
    public class HESBase : OwningComponentBase
    {
        [Inject] protected IToastService ToastService { get; set; }

        protected bool Initialized { get; private set; }

        protected virtual void SetInitialized()
        {
            Initialized = true;
        }
    }
}