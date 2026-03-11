using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachTracker.api.Migrations
{
    /// <inheritdoc />
    public partial class RebuildInitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TrainingPlanDays",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "TargetSets",
                table: "TrainingPlanItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TargetReps",
                table: "TrainingPlanItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TrainingPlanDays",
                newName: "Title");

            migrationBuilder.AlterColumn<int>(
                name: "TargetSets",
                table: "TrainingPlanItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "TargetReps",
                table: "TrainingPlanItems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
