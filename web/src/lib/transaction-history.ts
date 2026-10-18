import type { Transaction } from '@/types/portal';

const TIME_ZONE = 'Africa/Johannesburg';
const DATE_PATTERN = /^\d{4}-\d{2}-\d{2}$/;

export type TransactionDateRange = {
  from: string;
  to: string;
  minimumDate: string;
  maximumDate: string;
  hasCustomFilters: boolean;
};

export type TransactionMonthGroup = {
  key: string;
  label: string;
  transactions: Transaction[];
};

function dateInTimeZone(value: Date): string {
  const parts = new Intl.DateTimeFormat('en-CA', {
    timeZone: TIME_ZONE,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  }).formatToParts(value);
  const fields = new Map(parts.map((part) => [part.type, part.value]));
  return `${fields.get('year')}-${fields.get('month')}-${fields.get('day')}`;
}

function isCalendarDate(value: unknown): value is string {
  if (typeof value !== 'string' || !DATE_PATTERN.test(value)) return false;
  const [year, month, day] = value.split('-').map(Number);
  const parsed = new Date(Date.UTC(year, month - 1, day));
  return (
    parsed.getUTCFullYear() === year &&
    parsed.getUTCMonth() === month - 1 &&
    parsed.getUTCDate() === day
  );
}

function subtractCalendarMonths(value: string, months: number): string {
  const [year, month, day] = value.split('-').map(Number);
  const targetMonth = new Date(Date.UTC(year, month - 1 - months, 1));
  const lastDayOfTargetMonth = new Date(
    Date.UTC(targetMonth.getUTCFullYear(), targetMonth.getUTCMonth() + 1, 0),
  ).getUTCDate();
  const targetDay = Math.min(day, lastDayOfTargetMonth);
  const yearText = targetMonth.getUTCFullYear().toString().padStart(4, '0');
  const monthText = (targetMonth.getUTCMonth() + 1).toString().padStart(2, '0');
  const dayText = targetDay.toString().padStart(2, '0');
  return `${yearText}-${monthText}-${dayText}`;
}

export function resolveTransactionDateRange(
  requestedFrom: unknown,
  requestedTo: unknown,
  now = new Date(),
): TransactionDateRange {
  const maximumDate = dateInTimeZone(now);
  const minimumDate = subtractCalendarMonths(maximumDate, 3);
  const hasCustomFilters = isCalendarDate(requestedFrom) || isCalendarDate(requestedTo);
  const requestedStart = isCalendarDate(requestedFrom) ? requestedFrom : minimumDate;
  const requestedEnd = isCalendarDate(requestedTo) ? requestedTo : maximumDate;
  let from = requestedStart < minimumDate ? minimumDate : requestedStart;
  let to = requestedEnd > maximumDate ? maximumDate : requestedEnd;

  if (to < minimumDate) to = minimumDate;
  if (from > maximumDate) from = maximumDate;
  if (from > to) from = to;

  return { from, to, minimumDate, maximumDate, hasCustomFilters };
}

export function groupTransactionsByMonth(transactions: Transaction[]): TransactionMonthGroup[] {
  const groups = new Map<string, TransactionMonthGroup>();

  for (const transaction of transactions) {
    const date = new Date(transaction.occurredAt);
    const parts = new Intl.DateTimeFormat('en-CA', {
      timeZone: TIME_ZONE,
      year: 'numeric',
      month: '2-digit',
    }).formatToParts(date);
    const fields = new Map(parts.map((part) => [part.type, part.value]));
    const year = fields.get('year')!;
    const month = fields.get('month')!;
    const key = `${year}-${month}`;
    let group = groups.get(key);

    if (!group) {
      group = {
        key,
        label: new Intl.DateTimeFormat('en-ZA', {
          timeZone: TIME_ZONE,
          month: 'long',
          year: 'numeric',
        }).format(date),
        transactions: [],
      };
      groups.set(key, group);
    }

    group.transactions.push(transaction);
  }

  return [...groups.values()];
}
