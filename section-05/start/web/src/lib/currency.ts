export const CURRENCY_LOCALE = "de-DE";

export const CURRENCIES = ["HUF", "EUR", "USD"] as const;
export type CurrencyCode = (typeof CURRENCIES)[number];
export const DEFAULT_CURRENCY: CurrencyCode = "HUF";

const formatters = Object.fromEntries(
  CURRENCIES.map((currency) => [currency, new Intl.NumberFormat(CURRENCY_LOCALE, { style: "currency", currency })]),
) as Record<CurrencyCode, Intl.NumberFormat>;

export function formatCurrency(amount: number, currency: CurrencyCode): string {
  return formatters[currency].format(amount);
}
