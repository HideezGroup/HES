using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_proximity_vault_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("WorkstationProximityVaults", newName: "WorkstationHardwareVaultPairs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("WorkstationHardwareVaultPairs", newName: "WorkstationProximityVaults");
        }
    }
}