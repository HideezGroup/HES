using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Models.Dashboard
{
    public class DashboardCard
    {
        public string CardTitle { get; set; }
        public string CardLogo { get; set; }
        public string LeftText { get; set; }
        public string LeftValue { get; set; }
        public string LeftLink { get; set; }
        public string RightText { get; set; }
        public string RightValue { get; set; }
        public string RightLink { get; set; }
        public List<DashboardNotify> Notifications { get; set; }
        public Func<Task> LeftAction { get; set; }
        public Func<Task> RightAction { get; set; }
    }
}