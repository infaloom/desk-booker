using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskManagementApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReservationUniqueConstraintUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Date_DeskId_Type",
                table: "Reservations",
                columns: new[] { "Date", "DeskId", "Type" },
                unique: true,
                filter: "[Type] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Date_UserId_Type",
                table: "Reservations",
                columns: new[] { "Date", "UserId", "Type" },
                unique: true,
                filter: "[Type] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_Date_DeskId_Type",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Date_UserId_Type",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
