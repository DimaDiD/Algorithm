using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMSA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AiPoints",
                table: "SubmissionAnswer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "SubmissionAnswer",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "SubmissionAnswer",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserId",
                table: "SubmissionAnswer",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherComment",
                table: "SubmissionAnswer",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionAnswer_ReviewStatus",
                table: "SubmissionAnswer",
                column: "ReviewStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubmissionAnswer_ReviewStatus",
                table: "SubmissionAnswer");

            migrationBuilder.DropColumn(
                name: "AiPoints",
                table: "SubmissionAnswer");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "SubmissionAnswer");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "SubmissionAnswer");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "SubmissionAnswer");

            migrationBuilder.DropColumn(
                name: "TeacherComment",
                table: "SubmissionAnswer");
        }
    }
}
