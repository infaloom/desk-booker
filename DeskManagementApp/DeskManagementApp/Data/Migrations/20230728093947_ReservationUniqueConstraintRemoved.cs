using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskManagementApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReservationUniqueConstraintRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_Date_DeskId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Date_UserId",
                table: "Reservations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Date_DeskId",
                table: "Reservations",
                columns: new[] { "Date", "DeskId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Date_UserId",
                table: "Reservations",
                columns: new[] { "Date", "UserId" },
                unique: true);
        }
    }
}
