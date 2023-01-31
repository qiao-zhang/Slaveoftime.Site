using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slaveoftime.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddContentPathToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentPath",
                table: "Posts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentPath",
                table: "Posts");
        }
    }
}
