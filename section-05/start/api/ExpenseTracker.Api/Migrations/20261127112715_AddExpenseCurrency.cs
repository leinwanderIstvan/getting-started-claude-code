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
            // Existing rows are backfilled with HUF through a temporary default: on Postgres 11+
            // adding a NOT NULL column with a constant default is metadata-only (no row rewrite).
            // The default is dropped straight away; the API always sets Currency explicitly.
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "HUF");

            migrationBuilder.Sql("""ALTER TABLE "Expenses" ALTER COLUMN "Currency" DROP DEFAULT;""");

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
