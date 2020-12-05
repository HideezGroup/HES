using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_login_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Kind",
                table: "SharedAccounts",
                newName: "LoginType");

            migrationBuilder.RenameColumn(
               name: "Kind",
               table: "Accounts",
               newName: "LoginType");

            migrationBuilder.Sql(
               @"
                 UPDATE SharedAccounts
                        SET LoginType = (CASE
                            WHEN Login Like '.\\\%' THEN '1'
                            WHEN Login LIKE '@\\\%' THEN '4'
                            WHEN Login LIKE 'AzureAD\\\%' THEN '3'
                            WHEN Login LIKE '_%\\\%_' THEN '2'
                            ELSE '0' END)
                ");

            migrationBuilder.Sql(
              @"
                 UPDATE Accounts
                        SET LoginType = (CASE
                            WHEN Login Like '.\\\%' THEN '1'
                            WHEN Login LIKE '@\\\%' THEN '4'
                            WHEN Login LIKE 'AzureAD\\\%' THEN '3'
                            WHEN Login LIKE '_%\\\%_' THEN '2'
                            ELSE '0' END)
                ");

            //migrationBuilder.DropColumn(
            //    name: "Kind",
            //    table: "SharedAccounts");

            //migrationBuilder.DropColumn(
            //    name: "Kind",
            //    table: "Accounts");

            //migrationBuilder.AddColumn<int>(
            //    name: "LoginType",
            //    table: "SharedAccounts",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "LoginType",
            //    table: "Accounts",
            //    nullable: false,
            //    defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
               name: "LoginType",
               table: "SharedAccounts",
               newName: "Kind");

            migrationBuilder.RenameColumn(
                name: "LoginType",
                table: "Accounts",
                newName: "Kind");


            //migrationBuilder.DropColumn(
            //    name: "LoginType",
            //    table: "SharedAccounts");

            //migrationBuilder.DropColumn(
            //    name: "LoginType",
            //    table: "Accounts");

            //migrationBuilder.AddColumn<int>(
            //    name: "Kind",
            //    table: "SharedAccounts",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Kind",
            //    table: "Accounts",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);
        }
    }
}
