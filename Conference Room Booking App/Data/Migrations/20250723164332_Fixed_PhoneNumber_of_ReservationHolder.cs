using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conference_Room_Booking_App.Migrations
{
    /// <inheritdoc />
    public partial class Fixed_PhoneNumber_of_ReservationHolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "IsAppUser",
            //    table: "ReservationHolders");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "ReservationHolders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PhoneNumber",
                table: "ReservationHolders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsAppUser",
            //    table: "ReservationHolders",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);
        }
    }
}
