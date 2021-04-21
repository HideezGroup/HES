using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.HardwareVaults;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace HES.Tests.Helpers
{
    public class HardwareVaultServiceTestingOptions
    {
        public int ActivationCodeLenght { get; private set; }
        public string HardwareVaultId { get; private set; }
        public string EmployeetId { get; private set; }
        public string NewHardwareVaultRFID { get; private set; }
        public HardwareVault HardwareVault { get; private set; }
        public int HardwareVaultsCount { get; private set; }
        public DataLoadingOptions<HardwareVaultFilter> DataLoadingOptions { get; private set; }
        public List<HardwareVault> TestingHardwareVaults { get; private set; }

        private readonly IApplicationDbContext _dbContext;
        
        public HardwareVaultServiceTestingOptions(int hardwareVaultsCount, int hardwareVaultId, string newHardwareVaultRFID, int employeeId, IApplicationDbContext dbContext)
        {
            NewHardwareVaultRFID = newHardwareVaultRFID;
            _dbContext = dbContext;
            HardwareVaultId = hardwareVaultId.ToString();
            HardwareVaultsCount = hardwareVaultsCount;    
            EmployeetId = $"{employeeId}";

            _dbContext.HardwareVaultProfiles.Add(new HardwareVaultProfile
            {
                Id = ServerConstants.DefaulHardwareVaultProfileId,
                Name = "Default"
            });
            _dbContext.SaveChangesAsync();

            ActivationCodeLenght = 6;

            CreateHardwareVaults();

            DataLoadingOptions = new DataLoadingOptions<HardwareVaultFilter>()
            {
                Skip = 0,
                Take = HardwareVaultsCount,
                SortDirection = ListSortDirection.Ascending,
                SortedColumn = nameof(Employee.Id),
                SearchText = string.Empty,
                Filter = null
            };
        }

        private async void CreateHardwareVaults()
        {
            TestingHardwareVaults = new List<HardwareVault>();

            for (int i = 0; i < HardwareVaultsCount; i++)
            {
                var hardwareVault = new HardwareVault
                {
                    Id = $"{i}",
                    MAC = $"Q{i}:W{i}:E{i}",
                    Model = "ST102",
                    RFID = $"{i}{i}{i}",
                    Firmware = "3.5.2",
                    HardwareVaultProfileId = ServerConstants.DefaulHardwareVaultProfileId,
                    Status = VaultStatus.Ready,
                    ImportedAt = DateTime.Now
                };

                TestingHardwareVaults.Add(hardwareVault);

                if (hardwareVault.Id == HardwareVaultId)
                {
                    HardwareVault = hardwareVault;
                    hardwareVault.EmployeeId = EmployeetId;
                }
            }

            _dbContext.HardwareVaults.AddRange(TestingHardwareVaults);
            await _dbContext.SaveChangesAsync();

        }
    }
}