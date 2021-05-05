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
    }
}