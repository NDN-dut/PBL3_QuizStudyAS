using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizStudyAS.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // 1. TẠO BẢNG TRƯỚC
            migrationBuilder.CreateTable(
                name: "AuthProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthProviders", x => x.Id);
                });

            // 2. CHÈN DỮ LIỆU VÀO BẢNG VỪA TẠO
            migrationBuilder.InsertData(
                table: "AuthProviders",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Local" },
                    { 2, "Google" }
                });

            // 3. THÊM CỘT VÀO BẢNG USERS VỚI GIÁ TRỊ MẶC ĐỊNH LÀ 1 (Local)
            migrationBuilder.AddColumn<int>(
                name: "AuthProviderId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1); // Đã sửa defaultValue thành 1 để khớp với Id của Local

            // 4. TẠO INDEX VÀ KHÓA NGOẠI
            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthProviderId",
                table: "Users",
                column: "AuthProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AuthProviders_AuthProviderId",
                table: "Users",
                column: "AuthProviderId",
                principalTable: "AuthProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // THÊM DÒNG NÀY: Xóa dữ liệu mẫu trước khi sập bảng
            migrationBuilder.DeleteData(
                table: "AuthProviders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AuthProviders",
                keyColumn: "Id",
                keyValue: 2);
            // Các đoạn DropForeignKey và DropTable có sẵn của bạn (Giữ nguyên)

            migrationBuilder.DropForeignKey(
                name: "FK_Users_AuthProviders_AuthProviderId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AuthProviders");

            migrationBuilder.DropIndex(
                name: "IX_Users_AuthProviderId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthProviderId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
