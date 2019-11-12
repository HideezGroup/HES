﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models;
using Hideez.SDK.Communication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceService : IDeviceService
    {
        class MyHideezDevice
        {
            public string Id { get; set; }
            public string MAC { get; set; }
            public string ManufacturerUserId { get; set; }
            public string RFID { get; set; }
            public string Model { get; set; }
            public string BootLoaderVersion { get; set; }
            public DateTime Manufactured { get; set; }
            public string CpuSerialNo { get; set; }
            public byte[] DeviceKey { get; set; }
            public int? BleDeviceBatchId { get; set; }
            public string RegisteredUserId { get; set; }
        }

        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IDeviceAccessProfilesService _deviceAccessProfilesService;
        private readonly IAesCryptographyService _aesService;

        public DeviceService(IAsyncRepository<Device> deviceRepository,
                             IDeviceTaskService deviceTaskService,
                             IDeviceAccessProfilesService deviceAccessProfilesService,
                             IAesCryptographyService aesService)
        {
            _deviceRepository = deviceRepository;
            _deviceTaskService = deviceTaskService;
            _deviceAccessProfilesService = deviceAccessProfilesService;
            _aesService = aesService;
        }

        #region Device

        public IQueryable<Device> QueryOfDevice()
        {
            return _deviceRepository.Query();
        }

        public async Task<Device> GetDeviceByIdAsync(dynamic id)
        {
            return await _deviceRepository.GetByIdAsync(id);
        }

        public async Task<Device> GetByIdWithIncludeAsync(string id)
        {
            return await _deviceRepository
                .Query()
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Device>> GetDevicesAsync()
        {
            return await _deviceRepository
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(d => d.Employee.Department.Company)
                .ToListAsync();
        }

        public async Task<List<Device>> GetFilteredDevicesAsync(DeviceFilter deviceFilter)
        {
            var filter = _deviceRepository
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .Include(c => c.Employee.Department.Company)
                .AsQueryable();

            if (deviceFilter.Battery != null)
            {
                filter = filter.Where(w => w.Battery == deviceFilter.Battery);
            }
            if (deviceFilter.Firmware != null)
            {
                filter = filter.Where(w => w.Firmware.Contains(deviceFilter.Firmware));
            }
            if (deviceFilter.EmployeeId != null)
            {
                if (deviceFilter.EmployeeId == "N/A")
                {
                    filter = filter.Where(w => w.EmployeeId == null);
                }
                else
                {
                    filter = filter.Where(w => w.EmployeeId == deviceFilter.EmployeeId);
                }
            }
            if (deviceFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Employee.Department.Company.Id == deviceFilter.CompanyId);
            }
            if (deviceFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.Employee.DepartmentId == deviceFilter.DepartmentId);
            }
            if (deviceFilter.StartDate != null && deviceFilter.EndDate != null)
            {
                filter = filter.Where(w => w.LastSynced.HasValue
                                        && w.LastSynced.Value >= deviceFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime()
                                        && w.LastSynced.Value <= deviceFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }

            return await filter
                .OrderBy(w => w.Id)
                .Take(deviceFilter.Records)
                .ToListAsync();
        }

        public async Task<(IList<Device> devicesExists, IList<Device> devicesImported, string message)> ImportDevices(string key, byte[] fileContent)
        {
            IList<Device> devicesExists = null;
            IList<Device> devicesImported = null;
            string message = null;

            var objects = _aesService.DecryptObject<List<MyHideezDevice>>(fileContent, Encoding.Unicode.GetBytes(key));
            if (objects.Count > 0)
            {
                // Get all exists devices
                var isExist = await _deviceRepository.Query().Where(d => objects.Select(o => o.Id).Contains(d.Id)).ToListAsync();
                if (isExist.Count > 0)
                {
                    devicesExists = isExist;
                }
                // Devices to import
                var toImport = objects.Where(z => !isExist.Select(m => m.Id).Contains(z.Id)).Select(d => new Device()
                {
                    Id = d.Id,
                    MAC = d.MAC,
                    Model = d.Model,
                    RFID = d.RFID,
                    Battery = 100,
                    Firmware = "3.0.0",
                    LastSynced = null,
                    EmployeeId = null,
                    PrimaryAccountId = null,
                    MasterPassword = null,
                    AcceessProfileId = "default",
                    ImportedAt = DateTime.UtcNow
                    //UsePin = true
                })
                .ToList();

                // Add devices if count > 0
                if (toImport.Count > 0)
                {
                    // Save devices
                    await _deviceRepository.AddRangeAsync(toImport);
                    devicesImported = toImport;
                }

                return (devicesExists, devicesImported, message);
            }
            else
            {
                message = "File is recognized, but it is no devices to import. Check file structure and try again.";
                return (devicesExists, devicesImported, message);
            }
        }

        public async Task EditRfidAsync(Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "RFID" });
        }
        // ???
        public async Task UpdateOnlyPropAsync(Device device, string[] properties)
        {
            await _deviceRepository.UpdateOnlyPropAsync(device, properties);
        }

        public async Task UpdateDeviceInfoAsync(string deviceId, int battery, string firmware, bool locked)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new Exception($"Device not found, ID: {deviceId}");
            }

            device.Battery = battery;
            device.Firmware = firmware;
            //todo - add SetState(Device device, DeviceState newState)
            if (device.State == DeviceState.OK && locked)
                device.State = DeviceState.Locked;
            device.LastSynced = DateTime.UtcNow;

            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "Battery", "Firmware", "State", "LastSynced" });
        }

        public async Task UnlockPinAsync(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new Exception($"Device not found, ID: {deviceId}");
            }

            // Update device state
            device.State = DeviceState.PendingUnlock;
            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "State" });

            // Create task
            await _deviceTaskService.AddUnlockPinTaskAsync(device);
        }

        public async Task<bool> ExistAsync(Expression<Func<Device, bool>> predicate)
        {
            return await _deviceRepository.ExistAsync(predicate);
        }

        public async Task RemoveEmployeeAsync(string deviceId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);

            device.EmployeeId = null;
            device.PrimaryAccountId = null;
            device.MasterPassword = null;
            device.LastSynced = DateTime.UtcNow;

            var properties = new List<string>()
            {
                "EmployeeId",
                "PrimaryAccountId",
                "MasterPassword",
                "LastSynced"
            };

            await _deviceRepository.UpdateOnlyPropAsync(device, properties.ToArray());
        }

        #endregion

        #region Profile

        public async Task<string[]> GetDevicesByProfileAsync(string profileId)
        {
            var tasks = await _deviceTaskService
                .Query()
                .Where(d => d.Operation == TaskOperation.Wipe || d.Operation == TaskOperation.Link)
                .Select(s => s.DeviceId)
                .AsNoTracking()
                .ToListAsync();

            var devicesIds = await _deviceRepository
               .Query()
               .Where(d => d.AcceessProfileId == profileId && d.EmployeeId != null && !tasks.Contains(d.Id))
               .Select(s => s.Id)
               .AsNoTracking()
               .ToArrayAsync();

            return devicesIds;
        }

        public async Task SetProfileAsync(string[] devicesId, string profileId)
        {
            if (devicesId == null)
            {
                throw new ArgumentNullException(nameof(devicesId));
            }
            if (profileId == null)
            {
                throw new ArgumentNullException(nameof(profileId));
            }

            var profile = await _deviceAccessProfilesService.GetByIdAsync(profileId);
            if (profile == null)
            {
                throw new Exception("Profile not found");
            }

            foreach (var deviceId in devicesId)
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device != null)
                {
                    device.AcceessProfileId = profileId;
                    await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "AcceessProfileId" });

                    if (device.EmployeeId != null)
                    {
                        // Delete all previous tasks for update profile
                        await _deviceTaskService.RemoveAllProfileTasksAsync(device.Id);
                        // Add task for update profile
                        await _deviceTaskService.AddProfileTaskAsync(device);
                    }
                }
            }
        }

        public async Task<string[]> UpdateProfileAsync(string profileId)
        {
            var devicesId = await GetDevicesByProfileAsync(profileId);

            if (devicesId.Length > 0)
            {
                await SetProfileAsync(devicesId, profileId);
            }

            return devicesId;
        }

        #endregion
    }
}