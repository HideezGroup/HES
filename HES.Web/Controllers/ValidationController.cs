﻿using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidationController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public ValidationController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> VerifyEmailAsync([Bind(Prefix = "Employee.Email")]string email, [Bind(Prefix = "Employee.Id")]string id)
        {
            var employee = await _employeeService.Query().FirstOrDefaultAsync(e => e.Email == email && e.Id != id);

            if (employee != null)
            {
                return new JsonResult($"Email {email} already in use.");
            }

            return new JsonResult(true);
        }
    }
}