using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conference_Room_Booking_App.Migrations
{
    /// <inheritdoc />
    public partial class Made_AppUser_IdCardNumber_property_unique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdCardNumber",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IdCardNumber",
                table: "AspNetUsers",
                column: "IdCardNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IdCardNumber",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "IdCardNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
