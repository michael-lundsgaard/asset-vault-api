using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteBehaviorCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_UserProfiles_OwnerId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_UserProfiles_OwnerId",
                table: "Collections");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Collections",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Collections_OwnerId",
                table: "Collections",
                newName: "IX_Collections_UserId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Assets",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Assets_OwnerId",
                table: "Assets",
                newName: "IX_Assets_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Collections",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Collections",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_UserProfiles_UserId",
                table: "Assets",
                column: "UserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_UserProfiles_UserId",
                table: "Collections",
                column: "UserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_UserProfiles_UserId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_UserProfiles_UserId",
                table: "Collections");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Collections",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Collections_UserId",
                table: "Collections",
                newName: "IX_Collections_OwnerId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Assets",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Assets_UserId",
                table: "Assets",
                newName: "IX_Assets_OwnerId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Collections",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Collections",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_UserProfiles_OwnerId",
                table: "Assets",
                column: "OwnerId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_UserProfiles_OwnerId",
                table: "Collections",
                column: "OwnerId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
