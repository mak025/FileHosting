using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileHostingBackend.Migrations
{
    /// <inheritdoc />
    public partial class InviteRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Invites",
                newName: "InviteeEmail");

            migrationBuilder.AlterColumn<string>(
                name: "ShareLink",
                table: "StoredFiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_InvitedById",
                table: "Invites",
                column: "InvitedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Users_InvitedById",
                table: "Invites",
                column: "InvitedById",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Users_InvitedById",
                table: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_Invites_InvitedById",
                table: "Invites");

            migrationBuilder.RenameColumn(
                name: "InviteeEmail",
                table: "Invites",
                newName: "Email");

            migrationBuilder.AlterColumn<string>(
                name: "ShareLink",
                table: "StoredFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
