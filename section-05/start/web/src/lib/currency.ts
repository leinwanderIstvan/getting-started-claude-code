export const CURRENCY_LOCALE = "de-DE";

export const CURRENCIES = ["HUF", "EUR", "USD"] as const;
export type CurrencyCode = (typeof CURRENCIES)[number];
export const DEFAULT_CURRENCY: CurrencyCode = "HUF";

// Keyed by any ISO 4217 code, not just CURRENCIES: rows the API returns may carry a code
// this build does not offer in the form yet (e.g. a currency added server-side first).
const formatters = new Map<string, Intl.NumberFormat>();

export function formatCurrency(amount: number, currency: string): string {
  let formatter = formatters.get(currency);
  if (!formatter) {
    formatter = new Intl.NumberFormat(CURRENCY_LOCALE, { style: "currency", currency });
    formatters.set(currency, formatter);
  }
  return formatter.format(amount);
}
