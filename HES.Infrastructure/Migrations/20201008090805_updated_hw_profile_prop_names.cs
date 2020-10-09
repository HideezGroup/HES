using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class updated_hw_profile_prop_names : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ButtonBonding",
                table: "HardwareVaultProfiles",
                newName: "ButtonPairing");

            migrationBuilder.RenameColumn(
                name: "ButtonNewChannel",
                table: "HardwareVaultProfiles",
                newName: "ButtonStorageAccess");

            migrationBuilder.RenameColumn(
                name: "MasterKeyBonding",
                table: "HardwareVaultProfiles",
                newName: "MasterKeyPairing");

            migrationBuilder.RenameColumn(
                name: "MasterKeyNewChannel",
                table: "HardwareVaultProfiles",
                newName: "MasterKeyStorageAccess");

            migrationBuilder.RenameColumn(
                name: "PinBonding",
                table: "HardwareVaultProfiles",
                newName: "PinPairing");

            migrationBuilder.RenameColumn(
                name: "PinNewChannel",
                table: "HardwareVaultProfiles",
                newName: "PinStorageAccess");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ButtonPairing",
                table: "HardwareVaultProfiles",
                newName: "ButtonBonding");

            migrationBuilder.RenameColumn(
                name: "ButtonStorageAccess",
                table: "HardwareVaultProfiles",
                newName: "ButtonNewChannel");

            migrationBuilder.RenameColumn(
                name: "MasterKeyPairing",
                table: "HardwareVaultProfiles",
                newName: "MasterKeyBonding");

            migrationBuilder.RenameColumn(
                name: "MasterKeyStorageAccess",
                table: "HardwareVaultProfiles",
                newName: "MasterKeyNewChannel");

            migrationBuilder.RenameColumn(
                name: "PinPairing",
                table: "HardwareVaultProfiles",
                newName: "PinBonding");

            migrationBuilder.RenameColumn(
                name: "PinStorageAccess",
                table: "HardwareVaultProfiles",
                newName: "PinNewChannel");            
        }
    }
}
