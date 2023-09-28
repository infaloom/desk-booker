using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskManagementApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReservationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reservations");
        }
    }
}
