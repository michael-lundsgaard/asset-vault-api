using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Collections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "StoragePath",
                table: "Assets",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_OwnerId",
                table: "Collections",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_UserProfiles_OwnerId",
                table: "Collections",
                column: "OwnerId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_UserProfiles_OwnerId",
                table: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_Collections_OwnerId",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Collections");

            migrationBuilder.AlterColumn<string>(
                name: "StoragePath",
                table: "Assets",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true);
        }
    }
}
