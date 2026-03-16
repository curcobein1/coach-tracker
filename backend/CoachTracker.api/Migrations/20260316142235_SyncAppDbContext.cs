using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoachTracker.api.Migrations
{
    /// <inheritdoc />
    public partial class SyncAppDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "torso");

            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "limbs");

            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "upper");

            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "lower");

            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "anterior");

            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "posterior");

            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "push");

            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "pull");

            migrationBuilder.UpdateData(
                table: "SplitDays",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "legs");

            migrationBuilder.UpdateData(
                table: "Splits",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "torso / limbs");

            migrationBuilder.UpdateData(
                table: "Splits",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "upper / lower");

            migrationBuilder.UpdateData(
                table: "Splits",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "anterior / posterior");

            migrationBuilder.UpdateData(
                table: "Splits",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "push / pull / legs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Exercises;");
        }
    }
}
