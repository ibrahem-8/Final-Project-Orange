using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopStudentsTutoringPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationalPackageSubjectAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EducationalPackages",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EducationalPackages",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EducationalPackages",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "EducationalPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EducationalPackages_SubjectId",
                table: "EducationalPackages",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalPackages_Subjects_SubjectId",
                table: "EducationalPackages",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalPackages_Subjects_SubjectId",
                table: "EducationalPackages");

            migrationBuilder.DropIndex(
                name: "IX_EducationalPackages_SubjectId",
                table: "EducationalPackages");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EducationalPackages");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "EducationalPackages");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EducationalPackages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EducationalPackages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
