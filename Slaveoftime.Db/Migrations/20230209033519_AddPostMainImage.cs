using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slaveoftime.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddPostMainImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainImage",
                table: "Posts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainImage",
                table: "Posts");
        }
    }
}
