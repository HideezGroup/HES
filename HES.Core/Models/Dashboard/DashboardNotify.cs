using System;
using System.Threading.Tasks;

namespace HES.Core.Models
{
    public class DashboardNotify
    {
        public string Message { get; set; }
        public int Count { get; set; }
        public string Page { get; set; }
        public Func<Task> Action { get; set; }
    }
}