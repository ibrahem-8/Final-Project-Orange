using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopStudentsTutoringPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorDocumentsAndSubjectGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "TutorSubjects",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AcademicTranscriptUrl",
                table: "TutorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVUrl",
                table: "TutorProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "TutorSubjects");

            migrationBuilder.DropColumn(
                name: "AcademicTranscriptUrl",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "CVUrl",
                table: "TutorProfiles");
        }
    }
}
