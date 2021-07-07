using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HES.Web.Extensions
{
    public static class JSRuntimeExtentions
    {
        public static async Task<T> InvokeWebApiPostAsync<T>(this IJSRuntime jsRuntime, string url, object data)
        {
            var json = await jsRuntime.InvokeAsync<string>("postAsync", url, JsonConvert.SerializeObject(data));
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task InvokeWebApiPostVoidAsync(this IJSRuntime jsRuntime, string url, object data)
        {
            await jsRuntime.InvokeAsync<string>("postAsync", url, JsonConvert.SerializeObject(data));
        }

        public static async Task InvokeWebApiPostVoidAsync(this IJSRuntime jsRuntime, string url)
        {
            await jsRuntime.InvokeAsync<string>("postAsync", url);
        }

        public static async Task<T> InvokeWebApiGetAsync<T>(this IJSRuntime jsRuntime, string url)
        {
            var json = await jsRuntime.InvokeAsync<string>("getAsync", url);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}