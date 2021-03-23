using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_groups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "GroupMemberships",
            //    columns: table => new
            //    {
            //        GroupId = table.Column<string>(nullable: false),
            //        EmployeeId = table.Column<string>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_GroupMemberships", x => new { x.GroupId, x.EmployeeId });
            //    });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            // ==== Fixed GroupMemberships
            migrationBuilder.CreateTable(
               name: "GroupMemberships",
               columns: table => new
               {
                   Id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                   GroupId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
                   EmployeeId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_GroupMemberships", x => x.Id);
                   table.ForeignKey(
                       name: "FK_GroupMemberships_Employees_EmployeeId",
                       column: x => x.EmployeeId,
                       principalTable: "Employees",
                       principalColumn: "Id",
                       onDelete: ReferentialAction.Cascade);
                   table.ForeignKey(
                       name: "FK_GroupMemberships_Groups_GroupId",
                       column: x => x.GroupId,
                       principalTable: "Groups",
                       principalColumn: "Id",
                       onDelete: ReferentialAction.Cascade);
               });
            // ====

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupMemberships");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
