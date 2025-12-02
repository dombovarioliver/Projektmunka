using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaFit.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakePlanIdNullableInCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_DietPlans_PlanId",
                table: "Cases");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlanId",
                table: "Cases",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_DietPlans_PlanId",
                table: "Cases",
                column: "PlanId",
                principalTable: "DietPlans",
                principalColumn: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_DietPlans_PlanId",
                table: "Cases");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlanId",
                table: "Cases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_DietPlans_PlanId",
                table: "Cases",
                column: "PlanId",
                principalTable: "DietPlans",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
