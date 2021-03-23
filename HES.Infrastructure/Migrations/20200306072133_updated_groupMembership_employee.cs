using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class updated_groupMembership_employee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_GroupMemberships_Employees_EmployeeId",
            //    table: "GroupMemberships");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_GroupMemberships_Employees_EmployeeId",
            //    table: "GroupMemberships",
            //    column: "EmployeeId",
            //    principalTable: "Employees",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_GroupMemberships_Employees_EmployeeId",
            //    table: "GroupMemberships");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_GroupMemberships_Employees_EmployeeId",
            //    table: "GroupMemberships",
            //    column: "EmployeeId",
            //    principalTable: "Employees",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }
    }
}
