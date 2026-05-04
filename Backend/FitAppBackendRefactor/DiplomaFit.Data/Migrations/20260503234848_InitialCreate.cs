using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaFit.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NameHu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PrimaryMuscleGroup = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PrimaryMuscleSubgroup = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Pattern = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Equipment = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IsCompound = table.Column<bool>(type: "bit", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: false),
                    PushPullCategory = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MinExperienceLevel = table.Column<int>(type: "int", nullable: false),
                    DefaultSets = table.Column<int>(type: "int", nullable: false),
                    DefaultRepsLow = table.Column<int>(type: "int", nullable: false),
                    DefaultRepsHigh = table.Column<int>(type: "int", nullable: false),
                    IsHomeFriendly = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.ExerciseId);
                });

            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    FoodId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FoodNameHu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FoodNameEn = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    KcalPer100 = table.Column<double>(type: "float", nullable: false),
                    ProteinGPer100 = table.Column<double>(type: "float", nullable: false),
                    CarbsGPer100 = table.Column<double>(type: "float", nullable: false),
                    FatGPer100 = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.FoodId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    HeightCm = table.Column<int>(type: "int", nullable: false),
                    WeightKg = table.Column<double>(type: "float", nullable: false),
                    BodyfatPercent = table.Column<double>(type: "float", nullable: true),
                    ActivityLevel = table.Column<int>(type: "int", nullable: false),
                    GoalType = table.Column<int>(type: "int", nullable: false),
                    GoalDeltaKg = table.Column<int>(type: "int", nullable: false),
                    GoalTimeWeeks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
