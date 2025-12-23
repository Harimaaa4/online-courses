using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace online_courses.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "image",
                table: "categories",
                newName: "image_path");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "categories",
                newName: "image");
        }
    }
}
