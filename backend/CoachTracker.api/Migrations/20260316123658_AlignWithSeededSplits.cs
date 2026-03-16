using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoachTracker.api.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithSeededSplits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "smith machine", "incline press", "incline sm press", "upperchest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "pec-deck mach", "horizontal abbduction", "pec-deck", "chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "smith machine", "vertical press", "oh press", "side-delts" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Equipment", "Name", "PrimaryMuscle" },
                values: new object[] { "belt", "weighted p-up", "lats" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DefaultPlannedSets", "Equipment", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "machine", "l_pd(wg)", "lats" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "cable/bench", "overhead extension", "seated cable triceps oh extension", "tceps longhead" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "cable", "lateral raise", "cable cuff lraise", "side-delts" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "dumbell", "curl", "seated incline curl", "bceps" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "machine", "horizontal pull", "upper-back row", "upper-back" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "machine", "scapular regression", "kelso shrug", "traps" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "cable", "sagital pull", "narrow grip latfocusedsinglearm row", "lats" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "dumbell", "curl", "preachers", "bceps" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "pec-deck machine", "rear-delt iso", "buttaflies", "rear-delts" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "belt", "dips", "weighteddips", "chest" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 15,
                column: "DefaultPlannedSets",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "machine", "extension", "leg xtension", "quads" });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "smith machine", "squat pattern", "sm bulgarians", "legs" });


            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle" },
                values: new object[] { 2, "smith machine", "squat pattern", "sm hipaBBductor", "legs" });


            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "DefaultPlannedSets", "Equipment", "MovementPatternTag", "Name", "PrimaryMuscle", "SecondaryMuscles" },
                values: new object[,]
                {
                    { 19, 2, "smith machine", "squat pattern", "hipadDuctor", "legs", null },
                    { 20, 2, "smith machine", "squat pattern", "baxktension", "legs", null },
                    { 21, 2, "smith machine", "squat pattern", "calf", "legs", null },
                    { 22, 2, "smith machine", "squat pattern", "cable crunch", "legs", null },
                    { 23, 2, "smith machine", "squat pattern", "singlearm tpushdown", "legs", null },
                    { 24, 2, "smith machine", "squat pattern", "forearms(r/k)", "legs", null },
                    { 25, 2, "smith machine", "squat pattern", "takanakas", "legs", null },
                    { 26, 2, "smith machine", "squat pattern", "bushi", "legs", null },
                    { 27, 2, "smith machine", "squat pattern", "horshi", "legs", null },
                    { 28, 2, "smith machine", "squat pattern", "dogshi", "legs", null },
                    { 29, 2, "smith machine", "squat pattern", "yadi", "legs", null },
                    { 30, 2, "smith machine", "squat pattern", "yaddi", "legs", null },
                    { 31, 2, "smith machine", "squat pattern", "yadda", "legs", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Exercises;");

        }
    }
}
