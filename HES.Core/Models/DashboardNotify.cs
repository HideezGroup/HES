﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HES.Core.Models
{
    public class DashboardNotify
    {
        public string Message { get; set; }
        public int Count { get; set; }
        public string Page { get; set; }
        public string Handler { get; set; }
    }
}