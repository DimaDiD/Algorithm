using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMSA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "SubPage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Page",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "SubPage");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Page");
        }
    }
}
