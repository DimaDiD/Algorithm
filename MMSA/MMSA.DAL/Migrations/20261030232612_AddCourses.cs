using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMSA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Page",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Course_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Course",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "IsPublished", "OwnerId", "Slug", "Title" },
                values: new object[] { 1, new DateTime(2026, 10, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Задачі з поясненнями та готовими прикладами коду кількома мовами програмування.", true, null, "metody-rozrobky-alhorytmiv", "Методи розробки алгоритмів" });

            migrationBuilder.Sql(@"
UPDATE Course
SET OwnerId = (
    SELECT TOP 1 ur.UserId
    FROM AspNetUserRoles ur
    JOIN AspNetRoles r ON r.Id = ur.RoleId
    WHERE r.Name = 'Admin'
    ORDER BY ur.UserId)
WHERE Id = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_Page_CourseId",
                table: "Page",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Course_OwnerId",
                table: "Course",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Course_Slug",
                table: "Course",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Page_Course_CourseId",
                table: "Page",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Page_Course_CourseId",
                table: "Page");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropIndex(
                name: "IX_Page_CourseId",
                table: "Page");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Page");
        }
    }
}
