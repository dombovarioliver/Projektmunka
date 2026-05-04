using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaFit.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMealTypeToFood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MealType",
                table: "Foods",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MealType",
                table: "Foods");
        }
    }
}
