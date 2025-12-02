using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaFit.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodMealType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MealType",
                table: "Foods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
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
