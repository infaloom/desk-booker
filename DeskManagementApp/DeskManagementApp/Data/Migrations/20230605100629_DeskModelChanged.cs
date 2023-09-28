using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskManagementApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeskModelChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Floor",
                table: "Desks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Floor",
                table: "Desks");
        }
    }
}
