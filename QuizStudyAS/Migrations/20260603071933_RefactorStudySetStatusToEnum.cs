using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizStudyAS.Migrations
{
    /// <inheritdoc />
    public partial class RefactorStudySetStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StudySets");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "StudySets",
                type: "int",
                nullable: false,
                defaultValue: 1);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "CreatedAt",
            //    table: "Classrooms",
            //    type: "datetime2",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "UpdatedAt",
            //    table: "Classrooms",
            //    type: "datetime2",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "StudySetStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySetStatuses", x => x.StatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudySets_StatusId",
                table: "StudySets",
                column: "StatusId");

            // Chèn khối lệnh này vào giữa CreateIndex và AddForeignKey
            migrationBuilder.InsertData(
                table: "StudySetStatuses",
                columns: new[] { "StatusId", "Name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "DeletedByUser" },
                    { 3, "LockedByAdmin" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_StudySets_StudySetStatuses_StatusId",
                table: "StudySets",
                column: "StatusId",
                principalTable: "StudySetStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySets_StudySetStatuses_StatusId",
                table: "StudySets");

            migrationBuilder.DropTable(
                name: "StudySetStatuses");

            migrationBuilder.DropIndex(
                name: "IX_StudySets_StatusId",
                table: "StudySets");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "StudySets");

            //migrationBuilder.DropColumn(
            //    name: "CreatedAt",
            //    table: "Classrooms");

            //migrationBuilder.DropColumn(
            //    name: "UpdatedAt",
            //    table: "Classrooms");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StudySets",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
