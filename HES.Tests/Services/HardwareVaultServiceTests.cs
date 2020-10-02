using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Tests.Helpers;
using HES.Web;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace HES.Tests.Services
{
    [Order(2)]
    public class HardwareVaultServiceTests : IClassFixture<CustomWebAppFactory<Startup>>
    {
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly HardwareVaultServiceTestingOptions _testingOptions;
        public HardwareVaultServiceTests(CustomWebAppFactory<Startup> factory)
        {
            _hardwareVaultService = factory.GetHardwareVaultService();
            _testingOptions = new HardwareVaultServiceTestingOptions(100, 30, "1234567", 20, factory.GetHardwareVaultRepository(), factory.GetHardwareVaultProfileRepository());
        }


        //TODO ImportHardwarevaults

        [Fact, Order(2)]
        public async Task GetVaultsCountAsync()
        {
            var vaultsCount = await _hardwareVaultService.GetVaultsCountAsync(_testingOptions.DataLoadingOptions);

            Assert.Equal(_testingOptions.HardwareVaultsCount, vaultsCount);
        }

        [Fact, Order(3)]
        public async Task GetVaultsAsync()
        {

            var result = await _hardwareVaultService.GetVaultsAsync(_testingOptions.DataLoadingOptions);

            Assert.NotEmpty(result);
            Assert.Equal(_testingOptions.HardwareVaultsCount, result.Count);
        }

        [Fact, Order(4)]
        public async Task GetVaultByIdAsync()
        {
            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.NotNull(result);
            Assert.Equal(_testingOptions.HardwareVaultId, result.Id);
            Assert.Equal(_testingOptions.HardwareVault.MAC, result.MAC);
        }

        [Fact, Order(5)]
        public async Task EditRfidAsync()
        {
            var hardwareVault = _testingOptions.HardwareVault;
            hardwareVault.RFID = _testingOptions.NewHardwareVaultRFID;

            await _hardwareVaultService.UpdateVaultAsync(hardwareVault);

            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.NotNull(result);
            Assert.Equal(_testingOptions.HardwareVaultId, result.Id);
            Assert.Equal(hardwareVault.RFID, result.RFID);
        }

        [Fact, Order(5)]
        public async Task UnchangedVaultAsync()
        {
            var hardwareVault = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            hardwareVault.MAC = "testMac";

           await _hardwareVaultService.UnchangedVaultAsync(hardwareVault);

            Assert.Equal(_testingOptions.HardwareVault.MAC, hardwareVault.MAC);
        }

        [Fact, Order(6)]
        public async Task UpdateAfterWipe()
        {
            await _hardwareVaultService.SetReadyStatusAsync(_testingOptions.HardwareVault);

            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.True(result.HasNewLicense);
            Assert.Null(result.MasterPassword);
            Assert.Equal(VaultStatus.Ready, result.Status);
        }

        [Fact, Order(7)]
        public async Task GenerateVaultActivationAsync()
        {
            var result = await _hardwareVaultService.CreateVaultActivationAsync(_testingOptions.HardwareVaultId);

            Assert.NotNull(result.Id);
            Assert.Equal(_testingOptions.HardwareVaultId, result.VaultId);
            Assert.False(string.IsNullOrWhiteSpace(result.AcivationCode));
            Assert.True(result.AcivationCode.Length == _testingOptions.ActivationCodeLenght);
        }

        [Fact, Order(8)]
        public async Task GetVaultActivationCodeAsync()
        {
            var result = await _hardwareVaultService.GetVaultActivationCodeAsync(_testingOptions.HardwareVaultId);

            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact, Order(9)]
        public async Task UpdateVaultAsync()
        {
            var hardwareVault = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);
            hardwareVault.Status = VaultStatus.Locked;

            await _hardwareVaultService.UpdateVaultAsync(hardwareVault);

            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.Equal(VaultStatus.Locked, result.Status);
        }

        [Fact, Order(10)]
        public async Task ActivateVaultAsync()
        {
            await _hardwareVaultService.ActivateVaultAsync(_testingOptions.HardwareVaultId);

            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.Equal(VaultStatus.Suspended, result.Status);
            Assert.Equal(VaultStatusReason.None, result.StatusReason);
        }


        [Fact, Order(11)]
        public async Task UpdateRangeVaultsAsync()
        {
            var hardwareVault = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);
            hardwareVault.Status = VaultStatus.Active;

            await _hardwareVaultService.UpdateRangeVaultsAsync(new HardwareVault[] { hardwareVault });

            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.Equal(VaultStatus.Active, result.Status);
        }

        [Fact, Order(12)]
        public async Task SuspendVaultAsync()
        {
            await _hardwareVaultService.SuspendVaultAsync(_testingOptions.HardwareVaultId, "TestDescription");

            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.Equal(VaultStatus.Suspended, result.Status);
            Assert.Equal(VaultStatusReason.None, result.StatusReason);
            Assert.Equal("TestDescription", result.StatusDescription);
        }

        [Fact, Order(13)]
        public async Task VaultCompromisedAsync()
        {
            await _hardwareVaultService.VaultCompromisedAsync(_testingOptions.HardwareVaultId, VaultStatusReason.Broken, "TestDescription");

            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.Null(result.EmployeeId);
            Assert.Equal(VaultStatus.Compromised, result.Status);
            Assert.Equal(VaultStatusReason.Broken, result.StatusReason);
            Assert.Equal("TestDescription", result.StatusDescription);
        }

        [Fact, Order(14)]
        public async Task UpdateAfterWipeAsync()
        {
            await _hardwareVaultService.SetReadyStatusAsync(_testingOptions.HardwareVault);

            var result = await _hardwareVaultService.GetVaultByIdAsync(_testingOptions.HardwareVaultId);

            Assert.Null(result.MasterPassword);
            Assert.Equal(VaultStatus.Ready, result.Status);
            Assert.True(result.HasNewLicense);
        }
    }
}
