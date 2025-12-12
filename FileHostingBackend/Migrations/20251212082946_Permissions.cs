using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileHostingBackend.Migrations
{
    /// <inheritdoc />
    public partial class Permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredFiles_Users_UploadedByID",
                table: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_UploadedByID",
                table: "StoredFiles");

            migrationBuilder.CreateTable(
                name: "StoredFileUserPermissions",
                columns: table => new
                {
                    StoredFileInfoID = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFileUserPermissions", x => new { x.StoredFileInfoID, x.UserId });
                    table.ForeignKey(
                        name: "FK_StoredFileUserPermissions_StoredFiles_StoredFileInfoID",
                        column: x => x.StoredFileInfoID,
                        principalTable: "StoredFiles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoredFileUserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFileUserPermissions_UserId",
                table: "StoredFileUserPermissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoredFileUserPermissions");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UploadedByID",
                table: "StoredFiles",
                column: "UploadedByID");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredFiles_Users_UploadedByID",
                table: "StoredFiles",
                column: "UploadedByID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
