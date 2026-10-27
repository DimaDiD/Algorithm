using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMSA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Page",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Page", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    SubPageId = table.Column<int>(type: "int", nullable: true),
                    TextType = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    CodeType = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    ContentLocation = table.Column<int>(type: "int", nullable: true),
                    PageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubPage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubPage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubPage_Page",
                        column: x => x.PageId,
                        principalTable: "Page",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubPage_PageId",
                table: "SubPage",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageContent");

            migrationBuilder.DropTable(
                name: "SubPage");

            migrationBuilder.DropTable(
                name: "Page");
        }
    }
}
