using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Collections",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Collections");
        }
    }
}
