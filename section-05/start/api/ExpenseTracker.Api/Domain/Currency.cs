using System.Collections.Frozen;

namespace ExpenseTracker.Api.Domain;

/// <summary>
/// Supported ISO 4217 currency codes. Member names are the stored and wire values,
/// so they must stay uppercase three-letter codes.
/// </summary>
public enum Currency
{
    HUF,
    EUR,
    USD,
}

public static class CurrencyCodes
{
    public const Currency Default = Currency.HUF;

    private static readonly FrozenDictionary<string, Currency> ByCode =
        Enum.GetValues<Currency>().ToFrozenDictionary(c => c.ToString(), StringComparer.Ordinal);

    public static string Supported { get; } = string.Join(", ", Enum.GetNames<Currency>());

    /// <summary>
    /// Exact, case-sensitive match on the ISO code. Deliberately not <see cref="Enum.TryParse{TEnum}(string, out TEnum)"/>,
    /// which also accepts numbers, surrounding whitespace and comma-separated lists.
    /// </summary>
    public static bool TryParse(string code, out Currency currency) =>
        ByCode.TryGetValue(code, out currency);
}
