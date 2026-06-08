using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopStudentsTutoringPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationalPackageToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EducationalPackageId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EducationalPackageId",
                table: "Bookings",
                column: "EducationalPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_EducationalPackages_EducationalPackageId",
                table: "Bookings",
                column: "EducationalPackageId",
                principalTable: "EducationalPackages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_EducationalPackages_EducationalPackageId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_EducationalPackageId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EducationalPackageId",
                table: "Bookings");
        }
    }
}
