using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_fk_to_groupMembership : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_GroupMemberships_Groups_GroupId",
            //    table: "GroupMemberships");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_GroupMemberships",
            //    table: "GroupMemberships");

            //migrationBuilder.AlterColumn<string>(
            //    name: "EmployeeId",
            //    table: "GroupMemberships",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(255) CHARACTER SET utf8mb4");

            //migrationBuilder.AlterColumn<string>(
            //    name: "GroupId",
            //    table: "GroupMemberships",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "varchar(255) CHARACTER SET utf8mb4");

            //migrationBuilder.AddColumn<string>(
            //    name: "Id",
            //    table: "GroupMemberships",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_GroupMemberships",
            //    table: "GroupMemberships",
            //    column: "Id");

            //migrationBuilder.CreateIndex(
            //    name: "IX_GroupMemberships_EmployeeId",
            //    table: "GroupMemberships",
            //    column: "EmployeeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_GroupMemberships_GroupId",
            //    table: "GroupMemberships",
            //    column: "GroupId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_GroupMemberships_Employees_EmployeeId",
            //    table: "GroupMemberships",
            //    column: "EmployeeId",
            //    principalTable: "Employees",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_GroupMemberships_Groups_GroupId",
            //    table: "GroupMemberships",
            //    column: "GroupId",
            //    principalTable: "Groups",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_GroupMemberships_Employees_EmployeeId",
            //    table: "GroupMemberships");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_GroupMemberships_Groups_GroupId",
            //    table: "GroupMemberships");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_GroupMemberships",
            //    table: "GroupMemberships");

            //migrationBuilder.DropIndex(
            //    name: "IX_GroupMemberships_EmployeeId",
            //    table: "GroupMemberships");

            //migrationBuilder.DropIndex(
            //    name: "IX_GroupMemberships_GroupId",
            //    table: "GroupMemberships");

            //migrationBuilder.DropColumn(
            //    name: "Id",
            //    table: "GroupMemberships");

            //migrationBuilder.AlterColumn<string>(
            //    name: "GroupId",
            //    table: "GroupMemberships",
            //    type: "varchar(255) CHARACTER SET utf8mb4",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "EmployeeId",
            //    table: "GroupMemberships",
            //    type: "varchar(255) CHARACTER SET utf8mb4",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldNullable: true);

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_GroupMemberships",
            //    table: "GroupMemberships",
            //    columns: new[] { "GroupId", "EmployeeId" });

            //migrationBuilder.AddForeignKey(
            //    name: "FK_GroupMemberships_Groups_GroupId",
            //    table: "GroupMemberships",
            //    column: "GroupId",
            //    principalTable: "Groups",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }
    }
}
