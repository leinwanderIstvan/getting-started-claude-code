using ExpenseTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace ExpenseTracker.Tests;

/// <summary>
/// Each test gets its own throwaway database on the shared container (xUnit creates a new
/// instance per test), so migrating step by step never touches the API tests' database.
/// </summary>
public sealed class ExpenseCurrencyMigrationTests(PostgresApiFactory factory) : IAsyncDisposable
{
    private const string InitialCreate = "20260506140534_InitialCreate";

    private readonly ExpenseDbContext _db = CreateIsolatedContext(factory.ConnectionString);

    [Fact]
    public async Task Should_backfill_existing_expenses_with_HUF_when_migrating()
    {
        var ct = TestContext.Current.CancellationToken;
        var migrator = _db.GetService<IMigrator>();
        await migrator.MigrateAsync(InitialCreate, ct);

        var id = Guid.NewGuid();
        await _db.Database.ExecuteSqlAsync(
            $"""INSERT INTO "Expenses" ("Id", "Amount", "Description", "Date") VALUES ({id}, {12.50m}, {"Legacy row"}, {new DateOnly(2026, 1, 1)})""",
            ct);

        await migrator.MigrateAsync(cancellationToken: ct);

        var currency = await _db.Database
            .SqlQuery<string>($"""SELECT "Currency" AS "Value" FROM "Expenses" WHERE "Id" = {id}""")
            .SingleAsync(ct);

        Assert.Equal("HUF", currency);
    }

    [Fact]
    public async Task Should_reject_unsupported_currency_at_database_level()
    {
        var ct = TestContext.Current.CancellationToken;
        await _db.Database.MigrateAsync(ct);

        var exception = await Assert.ThrowsAsync<PostgresException>(() => _db.Database.ExecuteSqlAsync(
            $"""INSERT INTO "Expenses" ("Id", "Amount", "Description", "Date", "Currency") VALUES ({Guid.NewGuid()}, {5m}, {"Bad currency"}, {new DateOnly(2026, 1, 1)}, {"GBP"})""",
            ct));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
    }

    public async ValueTask DisposeAsync()
    {
        await _db.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
        await _db.DisposeAsync();
    }

    private static ExpenseDbContext CreateIsolatedContext(string containerConnectionString)
    {
        var connectionString = new NpgsqlConnectionStringBuilder(containerConnectionString)
        {
            Database = $"migration_test_{Guid.NewGuid():N}",
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<ExpenseDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ExpenseDbContext(options);
    }
}
