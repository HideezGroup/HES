using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_name_identifier_format_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameIdentifierFormat",
                table: "SamlRelyingParties",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE SamlRelyingParties SET NameIdentifierFormat = 'urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameIdentifierFormat",
                table: "SamlRelyingParties");
        }
    }
}
