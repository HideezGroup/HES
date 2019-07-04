﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationService _workstationService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Workstation> Workstations { get; set; }
        public Workstation Workstation { get; set; }
        public WorkstationFilter WorkstationFilter { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IWorkstationService workstationService, ILogger<IndexModel> logger)
        {
            _workstationService = workstationService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Workstations = await _workstationService
                .WorkstationQuery()
                .Include(c => c.Company)
                .Include(c => c.Department)
                .ToListAsync();

            ViewData["Companies"] = new SelectList(await _workstationService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["Departments"] = new SelectList(await _workstationService.DepartmentQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
        }

        public async Task<IActionResult> OnPostFilterWorkstationsAsync(WorkstationFilter WorkstationFilter)
        {
            var filter = _workstationService
                .WorkstationQuery()
                .Include(c => c.Company)
                .Include(c => c.Department)
                .AsQueryable();
           
            if (WorkstationFilter.Name != null)
            {
                filter = filter.Where(w => w.Name.Contains(WorkstationFilter.Name));
            }
            if (WorkstationFilter.ClientVersion != null)
            {
                filter = filter.Where(w => w.ClientVersion.Contains(WorkstationFilter.ClientVersion));
            }
            if (WorkstationFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == WorkstationFilter.CompanyId);
            }
            if (WorkstationFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == WorkstationFilter.DepartmentId);
            }
            if (WorkstationFilter.OS != null)
            {
                filter = filter.Where(w => w.OS.Contains(WorkstationFilter.OS));
            }
            if (WorkstationFilter.IP != null)
            {
                filter = filter.Where(w => w.IP.Contains(WorkstationFilter.IP));
            }
            if (WorkstationFilter.StartDate != null && WorkstationFilter.EndDate != null)
            {
                filter = filter
                    .Where(w => w.LastSeen.Date <= WorkstationFilter.EndDate.Value.Date.ToUniversalTime())
                    .Where(w => w.LastSeen.Date >= WorkstationFilter.StartDate.Value.Date.ToUniversalTime());
            }
            if (WorkstationFilter.Approved != null)
            {
                filter = filter.Where(w => w.Approved == WorkstationFilter.Approved);
            }

            Workstations = await filter.ToListAsync();

            return Partial("_WorkstationsTable", this);
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _workstationService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        }

        public async Task<IActionResult> OnGetEditDepartmentAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Workstation = await _workstationService
                .WorkstationQuery()
                .Include(c => c.Company)
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Workstation == null)
            {
                _logger.LogWarning("Computer == null");
                return NotFound();
            }

            ViewData["CompanyId"] = new SelectList(await _workstationService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(await _workstationService.DepartmentQuery().Where(d => d.CompanyId == Workstation.Department.CompanyId).ToListAsync(), "Id", "Name");

            return Partial("_EditDepartment", this);
        }

        public async Task<IActionResult> OnPostEditDepartmentAsync(Workstation Workstation)
        {
            if (Workstation == null)
            {
                _logger.LogWarning("departmentId == null");
                return RedirectToPage("./Index");
            }

            try
            {
                await _workstationService.EditDepartmentAsync(Workstation);
                SuccessMessage = $"Workstation updated.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetApproveComputerAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Workstation = await _workstationService
               .WorkstationQuery()
               .FirstOrDefaultAsync(c => c.Id == id);

            return Partial("_ApproveComputer", this);
        }

        public async Task<IActionResult> OnPostApproveComputerAsync(string computerId)
        {
            if (computerId == null)
            {
                _logger.LogWarning("computerId == null");
                return RedirectToPage("./Index");
            }
            try
            {
                await _workstationService.ApproveWorkstationAsync(computerId);
                SuccessMessage = $"Workstation approved.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }
    }
}