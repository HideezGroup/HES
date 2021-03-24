using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Template>>> GetTemplates()
        {
            return await _templateService.GetTemplatesAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Template>> GetTemplateById(string id)
        {
            var template = await _templateService.GetByIdAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            return template;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Template>> CreateTemplate(CreateTemplateDto templateDto)
        {
            Template createdTemplate;

            try
            {
                var template = new Template()
                {
                    Name = templateDto.Name,
                    Urls = templateDto.Urls,
                    Apps = templateDto.Apps
                };

                createdTemplate = await _templateService.CreateTmplateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetTemplateById", new { id = createdTemplate.Id }, createdTemplate);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditTemplate(string id, EditTemplateDto templateDto)
        {
            if (id != templateDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var template = new Template()
                {
                    Id = templateDto.Id,
                    Name = templateDto.Name,
                    Urls = templateDto.Urls,
                    Apps = templateDto.Apps
                };

                await _templateService.EditTemplateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Template>> DeleteTemplate(string id)
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            await _templateService.DeleteTemplateAsync(id);

            return template;
        }
    }
}
