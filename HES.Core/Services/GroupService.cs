using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Groups;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class GroupService : IGroupService
    {
        //private readonly IAsyncRepository<Group> _groupRepository;
        //private readonly IAsyncRepository<GroupMembership> _groupMembershipRepository;
        //private readonly IAsyncRepository<Employee> _employeeRepository;

        //public GroupService(IAsyncRepository<Group> groupRepository,
        //                    IAsyncRepository<GroupMembership> groupMembershipRepository,
        //                    IAsyncRepository<Employee> employeeRepository)
        //{
        //    _groupRepository = groupRepository;
        //    _groupMembershipRepository = groupMembershipRepository;
        //    _employeeRepository = employeeRepository;
        //}

        //public IQueryable<Group> Query()
        //{
        //    return _groupRepository.Query();
        //}

        //public async Task<List<Group>> GetGroupsAsync(DataLoadingOptions<GroupFilter> dataLoadingOptions)
        //{
        //    var query = _groupRepository
        //        .Query()
        //        .Include(x => x.GroupMemberships)
        //        .AsQueryable();

        //    // Filter
        //    if (dataLoadingOptions.Filter != null)
        //    {
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Name))
        //        {
        //            query = query.Where(x => x.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
        //        }
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Email))
        //        {
        //            query = query.Where(x => x.Email.Contains(dataLoadingOptions.Filter.Email, StringComparison.OrdinalIgnoreCase));
        //        }
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.MembersCount))
        //        {
        //            query = query.Where(x => x.GroupMemberships.Count().ToString().Contains(dataLoadingOptions.Filter.MembersCount));
        //        }
        //    }

        //    // Search
        //    if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
        //    {
        //        dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

        //        query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
        //                                 x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
        //                                 x.GroupMemberships.Count.ToString().Contains(dataLoadingOptions.SearchText));
        //    }

        //    // Sort Direction
        //    switch (dataLoadingOptions.SortedColumn)
        //    {
        //        case nameof(Group.Name):
        //            query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
        //            break;
        //        case nameof(Group.Email):
        //            query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email);
        //            break;
        //        case nameof(Group.GroupMemberships):
        //            query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.GroupMemberships.Count) : query.OrderByDescending(x => x.GroupMemberships.Count);
        //            break;
        //    }

        //    return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        //}

        //public async Task<int> GetGroupsCountAsync(DataLoadingOptions<GroupFilter> dataLoadingOptions)
        //{
        //    var query = _groupRepository.Query();

        //    // Filter
        //    if (dataLoadingOptions.Filter != null)
        //    {
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Name))
        //        {
        //            query = query.Where(x => x.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
        //        }
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Email))
        //        {
        //            query = query.Where(x => x.Email.Contains(dataLoadingOptions.Filter.Email, StringComparison.OrdinalIgnoreCase));
        //        }
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.MembersCount))
        //        {
        //            query = query.Where(x => x.GroupMemberships.Count().ToString().Contains(dataLoadingOptions.Filter.MembersCount));
        //        }
        //    }

        //    // Search
        //    if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
        //    {
        //        dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

        //        query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
        //                                 x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
        //                                 x.GroupMemberships.Count.ToString().Contains(dataLoadingOptions.SearchText));
        //    }

        //    return await query.CountAsync();
        //}

        //public async Task<Group> GetGroupByIdAsync(string groupId)
        //{
        //    return await _groupRepository.GetByIdAsync(groupId);
        //}

        //public async Task<Group> GetGroupByNameAsync(Group group)
        //{
        //    return await _groupRepository.Query().FirstOrDefaultAsync(x => x.Name == group.Name);
        //}

        //public async Task<Group> CreateGroupAsync(Group group)
        //{
        //    if (group == null)
        //    {
        //        throw new ArgumentNullException(nameof(group));
        //    }

        //    var exist = await _groupRepository.ExistAsync(x => x.Name == group.Name);

        //    if (exist)
        //    {
        //        throw new AlreadyExistException("This name is already in use.");
        //    }

        //    return await _groupRepository.AddAsync(group);
        //}

        //public async Task CreateGroupRangeAsync(List<Group> groups)
        //{
        //    foreach (var group in groups)
        //    {
        //        try
        //        {
        //            await CreateGroupAsync(group);
        //        }
        //        catch (AlreadyExistException)
        //        {
        //            // Continue, if group exist
        //        }
        //    }
        //}

        //public async Task EditGroupAsync(Group group)
        //{
        //    if (group == null)
        //    {
        //        throw new ArgumentNullException(nameof(group));
        //    }

        //    var exist = await _groupRepository.ExistAsync(x => x.Name == group.Name && x.Id != group.Id);

        //    if (exist)
        //    {
        //        throw new AlreadyExistException("This name is already in use.");
        //    }

        //    await _groupRepository.UpdateAsync(group);
        //}

        //public Task UnchangedGroupAsync(Group group)
        //{
        //    return _groupRepository.UnchangedAsync(group);
        //}

        //public async Task<Group> DeleteGroupAsync(string groupId)
        //{
        //    if (groupId == null)
        //    {
        //        throw new ArgumentNullException(nameof(groupId));
        //    }

        //    var group = await GetGroupByIdAsync(groupId);

        //    if (group == null)
        //    {
        //        throw new NotFoundException("Group not found.");
        //    }

        //    return await _groupRepository.DeleteAsync(group);
        //}

        //public async Task<List<GroupMembership>> GetGruopMembersAsync(string groupId)
        //{
        //    return await _groupMembershipRepository
        //        .Query()
        //        .Include(x => x.Employee)
        //        .Where(x => x.GroupId == groupId)
        //        .ToListAsync();
        //}

        //public async Task<List<GroupMembership>> GetGruopMembersAsync(DataLoadingOptions<GroupMembershipFilter> dataLoadingOptions)
        //{
        //    var query = _groupMembershipRepository
        //        .Query()
        //        .Include(x => x.Employee)
        //        .Where(x => x.GroupId == dataLoadingOptions.EntityId)
        //        .AsQueryable();

        //    // Filter
        //    if (dataLoadingOptions.Filter != null)
        //    {
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Name))
        //        {
        //            query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
        //        }
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Email))
        //        {
        //            query = query.Where(x => x.Employee.Email.Contains(dataLoadingOptions.Filter.Email, StringComparison.OrdinalIgnoreCase));
        //        }
        //    }

        //    // Search
        //    if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
        //    {
        //        dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

        //        query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
        //                                 x.Employee.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
        //    }

        //    // Sort Direction
        //    switch (dataLoadingOptions.SortedColumn)
        //    {
        //        case nameof(GroupMembership.Employee.FullName):
        //            query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
        //            break;
        //        case nameof(GroupMembership.Employee.Email):
        //            query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Email) : query.OrderByDescending(x => x.Employee.Email);
        //            break;
        //    }

        //    return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).ToListAsync();
        //}

        //public async Task<int> GetGruopMembersCountAsync(DataLoadingOptions<GroupMembershipFilter> dataLoadingOptions)
        //{
        //    var query = _groupMembershipRepository
        //        .Query()
        //        .Include(x => x.Employee)
        //        .Where(x => x.GroupId == dataLoadingOptions.EntityId)
        //        .AsQueryable();

        //    // Filter
        //    if (dataLoadingOptions.Filter != null)
        //    {
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Name))
        //        {
        //            query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
        //        }
        //        if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Email))
        //        {
        //            query = query.Where(x => x.Employee.Email.Contains(dataLoadingOptions.Filter.Email, StringComparison.OrdinalIgnoreCase));
        //        }
        //    }

        //    // Search
        //    if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
        //    {
        //        dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

        //        query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
        //                                 x.Employee.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
        //    }

        //    return await query.CountAsync();
        //}

        //public async Task<GroupMembership> GetGroupMembershipAsync(string employeeId, string groupId)
        //{
        //    return await _groupMembershipRepository
        //        .Query()
        //        .Include(x => x.Employee)
        //        .FirstOrDefaultAsync(x => x.GroupId == groupId && x.EmployeeId == employeeId);
        //}

        //public async Task<List<Employee>> GetEmployeesSkipExistingInGroupAsync(string groupId)
        //{
        //    var members = await GetGruopMembersAsync(groupId);

        //    return await _employeeRepository
        //        .Query()
        //        .Where(x => !members.Select(s => s.EmployeeId).Contains(x.Id))
        //        .ToListAsync();
        //}

        //public async Task AddEmployeesToGroupAsync(IList<string> employeeIds, string groupId)
        //{
        //    if (employeeIds == null)
        //    {
        //        throw new ArgumentNullException(nameof(employeeIds));
        //    }

        //    if (groupId == null)
        //    {
        //        throw new ArgumentNullException(nameof(groupId));
        //    }

        //    using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //    {
        //        foreach (var employeeId in employeeIds)
        //        {
        //            var employeeExist = await _groupMembershipRepository.ExistAsync(x => x.EmployeeId == employeeId && x.GroupId == groupId);
        //            if (employeeExist)
        //            {
        //                continue;
        //            }

        //            var groupMembership = new GroupMembership()
        //            {
        //                EmployeeId = employeeId,
        //                GroupId = groupId
        //            };

        //            await _groupMembershipRepository.AddAsync(groupMembership);
        //        }
        //        transactionScope.Complete();
        //    }
        //}

        //public async Task AddEmployeeToGroupsAsync(string employeeId, IList<string> groupIds)
        //{
        //    if (employeeId == null)
        //    {
        //        throw new ArgumentNullException(nameof(employeeId));
        //    }

        //    if (groupIds == null)
        //    {
        //        throw new ArgumentNullException(nameof(groupIds));
        //    }


        //    foreach (var groupId in groupIds)
        //    {
        //        var employeeExist = await _groupMembershipRepository.ExistAsync(x => x.EmployeeId == employeeId && x.GroupId == groupId);
        //        if (employeeExist)
        //        {
        //            continue;
        //        }

        //        var groupMembership = new GroupMembership()
        //        {
        //            EmployeeId = employeeId,
        //            GroupId = groupId
        //        };

        //        await _groupMembershipRepository.AddAsync(groupMembership);
        //    }
        //}

        //public async Task<GroupMembership> RemoveEmployeeFromGroupAsync(string groupMembershipId)
        //{
        //    if (groupMembershipId == null)
        //    {
        //        throw new ArgumentNullException(nameof(groupMembershipId));
        //    }

        //    var groupMembership = await _groupMembershipRepository.GetByIdAsync(groupMembershipId);
        //    if (groupMembership == null)
        //    {
        //        throw new NotFoundException("GroupMembership not found.");
        //    }

        //    return await _groupMembershipRepository.DeleteAsync(groupMembership);
        //}
    }
}