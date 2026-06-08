using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopStudentsTutoringPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorSubjectVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcademicTranscriptUrl",
                table: "TutorSubjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TutorSubjects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "TutorSubjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicTranscriptUrl",
                table: "TutorSubjects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TutorSubjects");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "TutorSubjects");
        }
    }
}
