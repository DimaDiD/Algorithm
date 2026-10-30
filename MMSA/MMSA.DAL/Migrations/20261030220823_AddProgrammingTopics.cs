using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MMSA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProgrammingTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgrammingTopic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgrammingTopic", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageContentTopic",
                columns: table => new
                {
                    PageContentId = table.Column<int>(type: "int", nullable: false),
                    ProgrammingTopicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContentTopic", x => new { x.PageContentId, x.ProgrammingTopicId });
                    table.ForeignKey(
                        name: "FK_PageContentTopic_PageContent_PageContentId",
                        column: x => x.PageContentId,
                        principalTable: "PageContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageContentTopic_ProgrammingTopic_ProgrammingTopicId",
                        column: x => x.ProgrammingTopicId,
                        principalTable: "ProgrammingTopic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProgrammingTopic",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Змінні та типи даних" },
                    { 2, "Введення та виведення даних" },
                    { 3, "Арифметичні операції" },
                    { 4, "Оператори порівняння" },
                    { 5, "Логічні оператори" },
                    { 6, "Умовний оператор if / else" },
                    { 7, "Оператор вибору switch / case" },
                    { 8, "Цикл for" },
                    { 9, "Цикл while" },
                    { 10, "Масиви" },
                    { 11, "Рядки" },
                    { 12, "Функції та процедури" },
                    { 13, "Рекурсія" },
                    { 14, "Анонімні функції" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageContentTopic_ProgrammingTopicId",
                table: "PageContentTopic",
                column: "ProgrammingTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammingTopic_Name",
                table: "ProgrammingTopic",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageContentTopic");

            migrationBuilder.DropTable(
                name: "ProgrammingTopic");
        }
    }
}
