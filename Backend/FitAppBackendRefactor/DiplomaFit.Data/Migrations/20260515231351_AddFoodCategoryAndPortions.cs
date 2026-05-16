using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaFit.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodCategoryAndPortions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FoodCategory",
                table: "Foods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MaxPortionGrams",
                table: "Foods",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinPortionGrams",
                table: "Foods",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FoodCategory",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "MaxPortionGrams",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "MinPortionGrams",
                table: "Foods");
        }
    }
}
