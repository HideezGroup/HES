﻿using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.HES.Pages
{
    public static class ManageNavPages
    {
        public static string Dashboard => "./Dashboard/Index";

        public static string Employees => "./Employees/Index";

        public static string Templates => "./Templates/Index";

        public static string Devices => "./Devices/Index";

        public static string Settings => "./Settings/Index";
               

        public static string DashboardNavClass(ViewContext viewContext) => PageNavClass(viewContext, Dashboard);

        public static string EmployeesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Employees);

        public static string TemplatesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Templates);

        public static string DevicesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Devices);

        public static string SettingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Settings);


        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}