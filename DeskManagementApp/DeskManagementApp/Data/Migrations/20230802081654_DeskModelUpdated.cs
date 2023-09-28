using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskManagementApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeskModelUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Desks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Desks");
        }
    }
}
