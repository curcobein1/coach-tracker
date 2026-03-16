using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoachTracker.api.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyWorkouts",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyWorkouts", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Equipment = table.Column<string>(type: "TEXT", nullable: true),
                    MovementPatternTag = table.Column<string>(type: "TEXT", nullable: true),
                    PrimaryMuscle = table.Column<string>(type: "TEXT", nullable: true),
                    SecondaryMuscles = table.Column<string>(type: "TEXT", nullable: true),
                    DefaultPlannedSets = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

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
                    FdcId = table.Column<int>(type: "INTEGER", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Splits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Splits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyWorkoutDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    SetNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    Reps = table.Column<int>(type: "INTEGER", nullable: false),
                    Rir = table.Column<int>(type: "INTEGER", nullable: true),
                    FormQuality = table.Column<string>(type: "TEXT", nullable: true),
                    LoggedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSets_DailyWorkouts_DailyWorkoutDate",
                        column: x => x.DailyWorkoutDate,
                        principalTable: "DailyWorkouts",
                        principalColumn: "Date",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutSets_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SplitDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SplitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SplitDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SplitDays_Splits_SplitId",
                        column: x => x.SplitId,
                        principalTable: "Splits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlannedCalendarDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    SplitDayId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedCalendarDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlannedCalendarDays_SplitDays_SplitDayId",
                        column: x => x.SplitDayId,
                        principalTable: "SplitDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SplitDayExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SplitDayId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetSets = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetRepRange = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SplitDayExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SplitDayExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SplitDayExercises_SplitDays_SplitDayId",
                        column: x => x.SplitDayId,
                        principalTable: "SplitDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle", "SecondaryMuscles" },
                values: new object[,]
                {
                    { 1, 3, "Barbell", "Horizontal Press", "Bench Press", "Pectorals", null },
                    { 2, 3, "Dumbbell", "Incline Press", "Incline Dumbbell Press", "Upper Pectorals", null },
                    { 3, 3, "Bodyweight", "Horizontal Press", "Push-ups", "Pectorals", null },
                    { 4, 3, "Bodyweight", "Vertical Pull", "Pull-ups", "Latissimus Dorsi", null },
                    { 5, 3, "Machine", "Vertical Pull", "Lat Pulldown", "Latissimus Dorsi", null },
                    { 6, 3, "Barbell", "Horizontal Pull", "Bent-over Row", "Rhomboids", null },
                    { 7, 3, "Barbell", "Squat", "Squats", "Quadriceps", null },
                    { 8, 3, "Barbell", "Hinge", "Romanian Deadlift", "Hamstrings", null },
                    { 9, 3, "Barbell", "Hip Extension", "Hip Thrust", "Glutes", null },
                    { 10, 3, "Machine", "Leg Press", "Leg Press", "Quadriceps", null },
                    { 11, 3, "Machine", "Calf Raise", "Calf Raises", "Calves", null },
                    { 12, 3, "Barbell", "Vertical Press", "Overhead Press", "Anterior Deltoids", null },
                    { 13, 3, "Dumbbell", "Shoulder Isolation", "Lateral Raises", "Lateral Deltoids", null },
                    { 14, 3, "Dumbbell", "Elbow Flexion", "Bicep Curls", "Biceps", null },
                    { 15, 3, "Cable", "Elbow Extension", "Tricep Extensions", "Triceps", null },
                    { 16, 3, "Bodyweight", "Core Anti-Extension", "Plank", "Abdominals", null },
                    { 17, 3, "Bodyweight", "Core Flexion", "Hanging Leg Raises", "Abdominals", null }
                });

            migrationBuilder.InsertData(
                table: "Splits",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Torso / Limbs" },
                    { 2, "Upper / Lower" },
                    { 3, "Anterior / Posterior" },
                    { 4, "Push / Pull / Legs" }
                });

            migrationBuilder.InsertData(
                table: "SplitDays",
                columns: new[] { "Id", "Name", "OrderIndex", "SplitId" },
                values: new object[,]
                {
                    { 1, "Torso", 0, 1 },
                    { 2, "Limbs", 1, 1 },
                    { 3, "Upper", 0, 2 },
                    { 4, "Lower", 1, 2 },
                    { 5, "Anterior", 0, 3 },
                    { 6, "Posterior", 1, 3 },
                    { 7, "Push", 0, 4 },
                    { 8, "Pull", 1, 4 },
                    { 9, "Legs", 2, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_NutritionFoodLogs_DayKey",
                table: "NutritionFoodLogs",
                column: "DayKey");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionUsuals_UpdatedAtUtc",
                table: "NutritionUsuals",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedCalendarDays_Date",
                table: "PlannedCalendarDays",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlannedCalendarDays_SplitDayId",
                table: "PlannedCalendarDays",
                column: "SplitDayId");

            migrationBuilder.CreateIndex(
                name: "IX_SplitDayExercises_ExerciseId",
                table: "SplitDayExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_SplitDayExercises_SplitDayId",
                table: "SplitDayExercises",
                column: "SplitDayId");

            migrationBuilder.CreateIndex(
                name: "IX_SplitDays_SplitId",
                table: "SplitDays",
                column: "SplitId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSets_DailyWorkoutDate",
                table: "WorkoutSets",
                column: "DailyWorkoutDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSets_ExerciseId",
                table: "WorkoutSets",
                column: "ExerciseId");
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

            migrationBuilder.DropTable(
                name: "PlannedCalendarDays");

            migrationBuilder.DropTable(
                name: "SplitDayExercises");

            migrationBuilder.DropTable(
                name: "WorkoutSets");

            migrationBuilder.DropTable(
                name: "SplitDays");

            migrationBuilder.DropTable(
                name: "DailyWorkouts");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Splits");
        }
    }
}
