using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conference_Room_Booking_App.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Booking_ReservationHolder_Appuser_to_support_authenticated_user_features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ReservationHolders_ReservationHolderId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_ReservationHolders_IdCardNumber",
                table: "ReservationHolders");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ReservationHolderId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "ReservationHolders");

            migrationBuilder.AlterColumn<string>(
                name: "IdCardNumber",
                table: "ReservationHolders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "ReservationHolderId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdCardNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AppUserId",
                table: "Bookings",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ReservationHolderId",
                table: "Bookings",
                column: "ReservationHolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_AppUserId",
                table: "Bookings",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ReservationHolders_ReservationHolderId",
                table: "Bookings",
                column: "ReservationHolderId",
                principalTable: "ReservationHolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_AppUserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ReservationHolders_ReservationHolderId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_AppUserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ReservationHolderId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IdCardNumber",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "IdCardNumber",
                table: "ReservationHolders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "ReservationHolders",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReservationHolderId",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationHolders_IdCardNumber",
                table: "ReservationHolders",
                column: "IdCardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ReservationHolderId",
                table: "Bookings",
                column: "ReservationHolderId",
                unique: true,
                filter: "[ReservationHolderId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ReservationHolders_ReservationHolderId",
                table: "Bookings",
                column: "ReservationHolderId",
                principalTable: "ReservationHolders",
                principalColumn: "Id");
        }
    }
}
