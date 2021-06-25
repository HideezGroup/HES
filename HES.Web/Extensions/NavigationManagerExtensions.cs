using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;

namespace HES.Web.Extensions
{
    public static class NavigationManagerExtensions
    {
        public static string GetQueryValue(this NavigationManager navigationManager, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var query = new Uri(navigationManager.Uri).Query;

            if (QueryHelpers.ParseQuery(query).TryGetValue(key, out var value))
                return value;

            return null;
        }

        public static Dictionary<string, string> GetQueryValues<T>(this NavigationManager navigationManager)
        {
            var queryValues = new Dictionary<string, string>();
            var props = typeof(T).GetProperties();

            foreach (var item in props)
            {
                var query = new Uri(navigationManager.Uri).Query;

                if (QueryHelpers.ParseQuery(query).TryGetValue(item.Name, out var value))
                {
                    queryValues.Add(item.Name, value);
                }
            }

            return queryValues;
        }

        public static string GetQueryString(this NavigationManager navigationManager)
        {
            return new Uri(navigationManager.Uri).Query;
        }

        public static void NavigateToLocal(this NavigationManager navigationManager, string uri, bool forceLoad = false)
        {
            if (IsLocalUrl(uri))
            {
                navigationManager.NavigateTo(uri, forceLoad);
            }
            else
            {
                navigationManager.NavigateTo(navigationManager.BaseUri);
            }
        }

        public static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // Allows "/" or "/foo" but not "//" or "/\".
            if (url[0] == '/')
            {
                // url is exactly "/"
                if (url.Length == 1)
                {
                    return true;
                }

                // url doesn't start with "//" or "/\"
                if (url[1] != '/' && url[1] != '\\')
                {
                    return true;
                }

                return false;
            }

            // Allows "~/" or "~/foo" but not "~//" or "~/\".
            if (url[0] == '~' && url.Length > 1 && url[1] == '/')
            {
                // url is exactly "~/"
                if (url.Length == 2)
                {
                    return true;
                }

                // url doesn't start with "~//" or "~/\"
                if (url[2] != '/' && url[2] != '\\')
                {
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}