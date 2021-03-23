using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class updated_groupMembership : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_GroupMemberships_Groups_GroupId",
            //    table: "GroupMemberships");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_GroupMemberships_Groups_GroupId",
            //    table: "GroupMemberships",
            //    column: "GroupId",
            //    principalTable: "Groups",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_GroupMemberships_Groups_GroupId",
            //    table: "GroupMemberships");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_GroupMemberships_Groups_GroupId",
            //    table: "GroupMemberships",
            //    column: "GroupId",
            //    principalTable: "Groups",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }
    }
}
