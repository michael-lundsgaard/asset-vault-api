using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailAndCoverUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Collections",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Assets",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Assets");
        }
    }
}
