﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationEventService : IWorkstationEventService
    {
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;

        public WorkstationEventService(IAsyncRepository<WorkstationEvent> workstationEventRepository,
                                       IAsyncRepository<Device> deviceRepository,
                                       IAsyncRepository<Employee> employeeRepository,
                                       IAsyncRepository<DeviceAccount> deviceAccountRepository)
        {
            _workstationEventRepository = workstationEventRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _deviceAccountRepository = deviceAccountRepository;
        }

        public IQueryable<WorkstationEvent> Query()
        {
            return _workstationEventRepository.Query();
        }

        public async Task AddEventAsync(WorkstationEvent workstationEvent)
        {
            if (workstationEvent == null)
                throw new ArgumentNullException(nameof(workstationEvent));

            await _workstationEventRepository.AddAsync(workstationEvent);
        }

        public async Task AddEventsRangeAsync(IList<WorkstationEventDto> workstationEventsDto)
        {
            if (workstationEventsDto == null)
                throw new ArgumentNullException(nameof(workstationEventsDto));

            var workstationEvents = new List<WorkstationEvent>();

            foreach (var workstationEventDto in workstationEventsDto)
            {
                string employeeId = null;
                string departmentId = null;
                string deviceAccountId = null;

                if (workstationEventDto.DeviceId != null)
                {
                    var device = await _deviceRepository.GetByIdAsync(workstationEventDto.DeviceId);
                    var employee = await _employeeRepository.GetByIdAsync(device.EmployeeId);
                    var deviceAccount = await _deviceAccountRepository
                        .Query()
                        .Where(d => d.Name == workstationEventDto.AccountName && d.Login == workstationEventDto.AccountLogin && d.DeviceId == workstationEventDto.DeviceId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    employeeId = device?.EmployeeId;
                    departmentId = employee?.DepartmentId;
                    deviceAccountId = deviceAccount?.Id;
                }

                var workstationEvent = new WorkstationEvent()
                {
                    Id = workstationEventDto.Id,
                    Date = workstationEventDto.Date,
                    EventId = workstationEventDto.EventId,
                    SeverityId = workstationEventDto.SeverityId,
                    Note = workstationEventDto.Note,
                    WorkstationId = workstationEventDto.WorkstationId,
                    UserSession = workstationEventDto.UserSession,
                    DeviceId = workstationEventDto.DeviceId,
                    EmployeeId = employeeId,
                    DepartmentId = departmentId,
                    DeviceAccountId = deviceAccountId,
                };

                workstationEvents.Add(workstationEvent);
            }
            await _workstationEventRepository.AddRangeAsync(workstationEvents);
        }
    }
}