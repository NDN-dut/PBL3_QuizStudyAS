using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizStudyAS.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLateToExamAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLate",
                table: "ExamAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLate",
                table: "ExamAttempts");
        }
    }
}
