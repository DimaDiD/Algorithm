using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMSA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseLanguage",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    ProgrammingLanguageId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseLanguage", x => new { x.CourseId, x.ProgrammingLanguageId });
                    table.ForeignKey(
                        name: "FK_CourseLanguage_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseLanguage_ProgrammingLanguage_ProgrammingLanguageId",
                        column: x => x.ProgrammingLanguageId,
                        principalTable: "ProgrammingLanguage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseLanguage_ProgrammingLanguageId",
                table: "CourseLanguage",
                column: "ProgrammingLanguageId");

            migrationBuilder.Sql(@"
INSERT INTO CourseLanguage (CourseId, ProgrammingLanguageId, [Order])
SELECT 1, Id, ROW_NUMBER() OVER (ORDER BY [Order], Id) - 1
FROM ProgrammingLanguage
WHERE IsActive = 1;");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProgrammingLanguage");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "ProgrammingLanguage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProgrammingLanguage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "ProgrammingLanguage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE pl
SET IsActive = CASE WHEN cl.ProgrammingLanguageId IS NULL THEN 0 ELSE 1 END,
    [Order] = COALESCE(cl.[Order], 100 + pl.Id)
FROM ProgrammingLanguage pl
LEFT JOIN CourseLanguage cl ON cl.ProgrammingLanguageId = pl.Id AND cl.CourseId = 1;");

            migrationBuilder.DropTable(
                name: "CourseLanguage");
        }
    }
}
