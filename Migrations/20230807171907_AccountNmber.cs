using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceManagementPro.Migrations
{
    public partial class AccountNmber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AccountNumber",
                table: "Customer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Customer");
        }
    }
}
