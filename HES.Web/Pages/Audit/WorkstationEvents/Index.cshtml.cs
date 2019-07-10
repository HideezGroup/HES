﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationEvents
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationEventService _workstationEventService;
        public IList<WorkstationEvent> WorkstationEvents { get; set; }
        public WorkstationEventFilter WorkstationEventFilter { get; set; }

        public IndexModel(IWorkstationEventService workstationEventService)
        {
            _workstationEventService = workstationEventService;
        }

        public async Task OnGetAsync()
        {
            WorkstationEvents = await _workstationEventService
                .WorkstationEventQuery()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .OrderByDescending(w => w.Date)
                .Take(100)
                .ToListAsync();

            ViewData["Events"] = new SelectList(Enum.GetValues(typeof(WorkstationEventId)).Cast<WorkstationEventId>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Severity"] = new SelectList(Enum.GetValues(typeof(WorkstationEventSeverity)).Cast<WorkstationEventSeverity>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Workstations"] = new SelectList(await _workstationEventService.WorkstationQuery().ToListAsync(), "Id", "Name");
            ViewData["Devices"] = new SelectList(await _workstationEventService.DeviceQuery().ToListAsync(), "Id", "Id");
            ViewData["Employees"] = new SelectList(await _workstationEventService.EmployeeQuery().ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _workstationEventService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["Departments"] = new SelectList(await _workstationEventService.DepartmentQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccounts"] = new SelectList(await _workstationEventService.DeviceAccountQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccountTypes"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
        }

        public async Task<IActionResult> OnPostFilterWorkstationEventsAsync(WorkstationEventFilter WorkstationEventFilter)
        {
            var filter = _workstationEventService
                 .WorkstationEventQuery()
                 .Include(w => w.Workstation)
                 .Include(w => w.Device)
                 .Include(w => w.Employee)
                 .Include(w => w.Department.Company)
                 .Include(w => w.DeviceAccount)
                 .OrderByDescending(w => w.Date)
                 .AsQueryable();

            if (WorkstationEventFilter.StartDate != null && WorkstationEventFilter.EndDate != null)
            {
                filter = filter
                    .Where(w => w.Date.Date <= WorkstationEventFilter.EndDate.Value.Date.ToUniversalTime())
                    .Where(w => w.Date.Date >= WorkstationEventFilter.StartDate.Value.Date.ToUniversalTime());
            }
            if (WorkstationEventFilter.EventId != null)
            {
                filter = filter.Where(w => w.EventId == (WorkstationEventId)WorkstationEventFilter.EventId);
            }
            if (WorkstationEventFilter.SeverityId != null)
            {
                filter = filter.Where(w => w.SeverityId == (WorkstationEventSeverity)WorkstationEventFilter.SeverityId);
            }
            if (WorkstationEventFilter.Note != null)
            {
                filter = filter.Where(w => w.Note.Contains(WorkstationEventFilter.Note));
            }
            if (WorkstationEventFilter.WorkstationId != null)
            {
                filter = filter.Where(w => w.WorkstationId == WorkstationEventFilter.WorkstationId);
            }
            if (WorkstationEventFilter.UserSession != null)
            {
                filter = filter.Where(w => w.UserSession.Contains(WorkstationEventFilter.UserSession));
            }
            if (WorkstationEventFilter.DeviceId != null)
            {
                filter = filter.Where(w => w.Device.Id == WorkstationEventFilter.DeviceId);
            }
            if (WorkstationEventFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.EmployeeId == WorkstationEventFilter.EmployeeId);
            }
            if (WorkstationEventFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == WorkstationEventFilter.CompanyId);
            }
            if (WorkstationEventFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == WorkstationEventFilter.DepartmentId);
            }
            if (WorkstationEventFilter.DeviceAccountId != null)
            {
                filter = filter.Where(w => w.DeviceAccountId == WorkstationEventFilter.DeviceAccountId);
            }
            if (WorkstationEventFilter.DeviceAccountTypeId != null)
            {
                filter = filter.Where(w => w.DeviceAccount.Type == (AccountType)WorkstationEventFilter.DeviceAccountTypeId);
            }

            WorkstationEvents = await filter.ToListAsync();

            return Partial("_WorkstationEventsTable", this);
        }
    }
}