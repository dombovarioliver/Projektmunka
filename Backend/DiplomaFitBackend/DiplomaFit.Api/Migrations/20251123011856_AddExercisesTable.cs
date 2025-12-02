using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaFit.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExercisesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameHu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrimaryMuscleGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrimaryMuscleSubgroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Pattern = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Equipment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCompound = table.Column<bool>(type: "bit", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: false),
                    PushPullCategory = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");
        }
    }
}
