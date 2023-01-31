using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slaveoftime.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDynamicToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDynamic",
                table: "Posts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDynamic",
                table: "Posts");
        }
    }
}
