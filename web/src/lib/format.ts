/** Keep all monetary arithmetic in integer minor units, including values above MAX_SAFE_INTEGER. */
export function formatMoney(amountMinor: string, currency: string): string {
  const options = new Intl.NumberFormat('en-ZA', { style: 'currency', currency }).resolvedOptions();
  const exponent = options.maximumFractionDigits ?? 2;
  const amount = BigInt(amountMinor);
  const negative = amount < BigInt(0);
  const absolute = negative ? -amount : amount;
  const divisor = BigInt(10) ** BigInt(exponent);
  const whole = absolute / divisor;
  const fraction = (absolute % divisor).toString().padStart(exponent, '0');
  const formatted = new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency,
    minimumFractionDigits: exponent,
    maximumFractionDigits: exponent,
  }).formatToParts(negative ? -whole : whole);
  const text = formatted.map((part) => (part.type === 'fraction' ? fraction : part.value)).join('');
  return negative && whole === BigInt(0) ? `-${text}` : text;
}
export function formatDate(value: string): string {
  return new Intl.DateTimeFormat('en-ZA', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    timeZone: 'Africa/Johannesburg',
  }).format(new Date(value));
}
