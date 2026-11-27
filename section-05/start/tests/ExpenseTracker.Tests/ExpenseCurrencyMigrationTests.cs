using ExpenseTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace ExpenseTracker.Tests;

public sealed class ExpenseCurrencyMigrationTests(PostgresApiFactory factory)
{
    private const string InitialCreate = "20260506140534_InitialCreate";

    [Fact]
    public async Task Should_backfill_existing_expenses_with_HUF_when_migrating()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateIsolatedContext();

        try
        {
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync(InitialCreate, ct);

            var id = Guid.NewGuid();
            await db.Database.ExecuteSqlAsync(
                $"""INSERT INTO "Expenses" ("Id", "Amount", "Description", "Date") VALUES ({id}, {12.50m}, {"Legacy row"}, {new DateOnly(2026, 1, 1)})""",
                ct);

            await migrator.MigrateAsync(cancellationToken: ct);

            var currency = await db.Database
                .SqlQuery<string>($"""SELECT "Currency" AS "Value" FROM "Expenses" WHERE "Id" = {id}""")
                .SingleAsync(ct);

            Assert.Equal("HUF", currency);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync(ct);
        }
    }

    [Fact]
    public async Task Should_reject_unsupported_currency_at_database_level()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var db = CreateIsolatedContext();

        try
        {
            await db.Database.MigrateAsync(ct);

            var exception = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlAsync(
                $"""INSERT INTO "Expenses" ("Id", "Amount", "Description", "Date", "Currency") VALUES ({Guid.NewGuid()}, {5m}, {"Bad currency"}, {new DateOnly(2026, 1, 1)}, {"GBP"})""",
                ct));

            Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync(ct);
        }
    }

    private ExpenseDbContext CreateIsolatedContext()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(factory.ConnectionString)
        {
            Database = $"migration_test_{Guid.NewGuid():N}",
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<ExpenseDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ExpenseDbContext(options);
    }
}
