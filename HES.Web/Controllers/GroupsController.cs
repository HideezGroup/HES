using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API.Group;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
        {
            _groupService = groupService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Group>>> GetGroups()
        {
            var count = await _groupService.GetGroupsCountAsync(new DataLoadingOptions<GroupFilter>());
            return await _groupService.GetGroupsAsync(new DataLoadingOptions<GroupFilter>
            {
                Take = count,
                SortedColumn = nameof(Group.Name),
                SortDirection = ListSortDirection.Ascending
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Group>> GetGroupById(string id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);

            if (group == null)
            {
                return NotFound();
            }

            return group;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Group>> CreateGroup(CreateGroupDto groupDto)
        {
            Group createdGroup;

            try
            {
                var group = new Group()
                {
                    Name = groupDto.Name,
                    Email = groupDto.Email,
                    Description = groupDto.Description
                };

                createdGroup = await _groupService.CreateGroupAsync(group);
            }
            catch (AlreadyExistException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }

            return CreatedAtAction("GetGroupById", new { id = createdGroup.Id }, createdGroup);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditGroup(string id, EditGroupDto groupDto)
        {
            if (id != groupDto.Id)
            {
                return BadRequest();
            }

            try
            {
                var group = new Group()
                {
                    Id = groupDto.Id,
                    Name = groupDto.Name,
                    Email = groupDto.Email,
                    Description = groupDto.Description
                };

                await _groupService.EditGroupAsync(group);
            }
            catch (AlreadyExistException ex)
            {
                return BadRequest(ex.Message);
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
        public async Task<ActionResult<Group>> DeleteGroup(string id)
        {
            try
            {
                return await _groupService.DeleteGroupAsync(id);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GroupMembership>>> GetGruopMembers(string id)
        {
            return await _groupService.GetGruopMembersAsync(id);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddEmployeesToGroup(AddEmployeesToGroupDto employeesToGroupDto)
        {
            try
            {
                await _groupService.AddEmployeesToGroupAsync(employeesToGroupDto.EmployeeIds, employeesToGroupDto.GroupId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddEmployeeToGroups(AddEmployeeToGroupsDto employeeToGroupsDto)
        {
            try
            {
                await _groupService.AddEmployeeToGroupsAsync(employeeToGroupsDto.EmployeeId, employeeToGroupsDto.GroupIds);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<GroupMembership>> GetGroupMembership(GroupMembershipDto groupMembershipDto)
        {
            return await _groupService.GetGroupMembershipAsync(groupMembershipDto.EmployeeId, groupMembershipDto.GroupId);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GroupMembership>> DeleteEmployeeFromGroup(string id)
        {
            try
            {
                return await _groupService.RemoveEmployeeFromGroupAsync(id);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}