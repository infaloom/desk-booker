using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskManagementApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeskOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Desks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Desks_UserId",
                table: "Desks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Desks_AspNetUsers_UserId",
                table: "Desks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Desks_AspNetUsers_UserId",
                table: "Desks");

            migrationBuilder.DropIndex(
                name: "IX_Desks_UserId",
                table: "Desks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Desks");
        }
    }
}
