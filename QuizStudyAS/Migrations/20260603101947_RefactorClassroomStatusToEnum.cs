using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuizStudyAS.Migrations
{
    /// <inheritdoc />
    public partial class RefactorClassroomStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Classrooms");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Classrooms",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ClassroomStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomStatuses", x => x.StatusId);
                });

            migrationBuilder.InsertData(
                table: "ClassroomStatuses",
                columns: new[] { "StatusId", "Name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "DeletedByUser" },
                    { 3, "LockedByAdmin" }
                });

            //migrationBuilder.InsertData(
            //    table: "StudySetStatuses",
            //    columns: new[] { "StatusId", "Name" },
            //    values: new object[,]
            //    {
            //        { 1, "Active" },
            //        { 2, "DeletedByUser" },
            //        { 3, "LockedByAdmin" }
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_StatusId",
                table: "Classrooms",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_ClassroomStatuses_StatusId",
                table: "Classrooms",
                column: "StatusId",
                principalTable: "ClassroomStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_ClassroomStatuses_StatusId",
                table: "Classrooms");

            migrationBuilder.DropTable(
                name: "ClassroomStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_StatusId",
                table: "Classrooms");

            //migrationBuilder.DeleteData(
            //    table: "StudySetStatuses",
            //    keyColumn: "StatusId",
            //    keyValue: 1);

            //migrationBuilder.DeleteData(
            //    table: "StudySetStatuses",
            //    keyColumn: "StatusId",
            //    keyValue: 2);

            //migrationBuilder.DeleteData(
            //    table: "StudySetStatuses",
            //    keyColumn: "StatusId",
            //    keyValue: 3);

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Classrooms");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Classrooms",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
