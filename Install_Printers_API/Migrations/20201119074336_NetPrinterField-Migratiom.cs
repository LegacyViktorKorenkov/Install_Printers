using Microsoft.EntityFrameworkCore.Migrations;

namespace Install_Printers_API.Migrations
{
    public partial class NetPrinterFieldMigratiom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NetPrinter",
                table: "Printers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetPrinter",
                table: "Printers");
        }
    }
}
