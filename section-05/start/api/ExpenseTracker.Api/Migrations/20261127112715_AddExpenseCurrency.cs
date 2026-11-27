using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add as nullable, backfill existing rows with HUF, then tighten to NOT NULL.
            // No lasting column default: the API always sets Currency explicitly.
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.Sql("""UPDATE "Expenses" SET "Currency" = 'HUF' WHERE "Currency" IS NULL;""");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Expenses_Currency",
                table: "Expenses",
                sql: "\"Currency\" IN ('HUF', 'EUR', 'USD')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Expenses_Currency",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Expenses");
        }
    }
}
