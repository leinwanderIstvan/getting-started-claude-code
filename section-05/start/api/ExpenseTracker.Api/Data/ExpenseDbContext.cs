using ExpenseTracker.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Data;

public sealed class ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : DbContext(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var expense = modelBuilder.Entity<Expense>();

        var supportedCurrencies = string.Join(", ", Enum.GetNames<Currency>().Select(code => $"'{code}'"));
        expense.ToTable(t => t.HasCheckConstraint(
            "CK_Expenses_Currency",
            $"\"Currency\" IN ({supportedCurrencies})"));

        expense.HasKey(e => e.Id);

        expense.Property(e => e.Amount)
            .HasPrecision(18, 2);

        expense.Property(e => e.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();

        expense.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(200);

        expense.Property(e => e.Date)
            .IsRequired();

        expense.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(now() at time zone 'utc')");

        expense.HasIndex(e => new { e.Date, e.CreatedAt })
            .IsDescending();
    }
}
