using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachTracker.api.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionAndKvTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyValues",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValues", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "NutritionFoodLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DayKey = table.Column<string>(type: "TEXT", nullable: false),
                    FdcId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Grams = table.Column<double>(type: "REAL", nullable: false),
                    Kcal = table.Column<double>(type: "REAL", nullable: false),
                    P = table.Column<double>(type: "REAL", nullable: false),
                    C = table.Column<double>(type: "REAL", nullable: false),
                    F = table.Column<double>(type: "REAL", nullable: false),
                    LoggedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionFoodLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NutritionUsuals",
                columns: table => new
                {
                    FdcId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Grams = table.Column<double>(type: "REAL", nullable: false),
                    Kcal = table.Column<double>(type: "REAL", nullable: false),
                    P = table.Column<double>(type: "REAL", nullable: false),
                    C = table.Column<double>(type: "REAL", nullable: false),
                    F = table.Column<double>(type: "REAL", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionUsuals", x => x.FdcId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NutritionFoodLogs_DayKey",
                table: "NutritionFoodLogs",
                column: "DayKey");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionUsuals_UpdatedAtUtc",
                table: "NutritionUsuals",
                column: "UpdatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyValues");

            migrationBuilder.DropTable(
                name: "NutritionFoodLogs");

            migrationBuilder.DropTable(
                name: "NutritionUsuals");
        }
    }
}
