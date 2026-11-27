export const CURRENCY_LOCALE = "de-DE";

export const CURRENCIES = ["HUF", "EUR", "USD"] as const;
export type CurrencyCode = (typeof CURRENCIES)[number];
export const DEFAULT_CURRENCY: CurrencyCode = "HUF";

const formatters = new Map<CurrencyCode, Intl.NumberFormat>();

function formatterFor(currency: CurrencyCode): Intl.NumberFormat {
  let formatter = formatters.get(currency);
  if (!formatter) {
    formatter = new Intl.NumberFormat(CURRENCY_LOCALE, { style: "currency", currency });
    formatters.set(currency, formatter);
  }
  return formatter;
}

export function formatCurrency(amount: number, currency: CurrencyCode): string {
  return formatterFor(currency).format(amount);
}

export function isCurrencyCode(value: string): value is CurrencyCode {
  return (CURRENCIES as readonly string[]).includes(value);
}
