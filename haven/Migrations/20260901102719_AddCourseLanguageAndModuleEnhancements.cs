using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haven.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseLanguageAndModuleEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Courses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionalMaterials",
                table: "CourseModules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionBn",
                table: "CourseModules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionEn",
                table: "CourseModules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "OptionalMaterials",
                table: "CourseModules");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionBn",
                table: "CourseModules");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionEn",
                table: "CourseModules");
        }
    }
}
