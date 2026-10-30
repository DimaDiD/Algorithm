using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MMSA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProgrammingLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgrammingLanguage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HighlightLanguage = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgrammingLanguage", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ProgrammingLanguage",
                columns: new[] { "Id", "Code", "HighlightLanguage", "IsActive", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "csharp", "csharp", true, "C#", 0 },
                    { 2, "cpp", "cpp", true, "C++", 1 },
                    { 3, "pascal", "delphi", true, "Pascal", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammingLanguage_Code",
                table: "ProgrammingLanguage",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgrammingLanguage");
        }
    }
}
