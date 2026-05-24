using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizStudyAS.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarUrlToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySets_Classrooms_ClassroomId",
                table: "StudySets");

            migrationBuilder.DropIndex(
                name: "IX_StudySets_ClassroomId",
                table: "StudySets");

            migrationBuilder.DropColumn(
                name: "ClassroomId",
                table: "StudySets");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Classrooms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ClassRoom_Material",
                columns: table => new
                {
                    ClassRoomId = table.Column<int>(type: "int", nullable: false),
                    StudySetId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRoom_Material", x => new { x.ClassRoomId, x.StudySetId });
                    table.ForeignKey(
                        name: "FK_ClassRoom_Material_Classrooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "Classrooms",
                        principalColumn: "ClassroomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassRoom_Material_StudySets_StudySetId",
                        column: x => x.StudySetId,
                        principalTable: "StudySets",
                        principalColumn: "StudySetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassRoom_Material_StudySetId",
                table: "ClassRoom_Material",
                column: "StudySetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassRoom_Material");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Classrooms");

            migrationBuilder.AddColumn<int>(
                name: "ClassroomId",
                table: "StudySets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudySets_ClassroomId",
                table: "StudySets",
                column: "ClassroomId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySets_Classrooms_ClassroomId",
                table: "StudySets",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "ClassroomId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
