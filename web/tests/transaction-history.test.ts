import { describe, expect, it } from 'vitest';
import {
  groupTransactionsByMonth,
  resolveTransactionDateRange,
} from '../src/lib/transaction-history';
import type { Transaction } from '../src/types/portal';

describe('transaction history date range', () => {
  it('uses the last three calendar months by default in Johannesburg time', () => {
    const range = resolveTransactionDateRange(
      undefined,
      undefined,
      new Date('2026-10-18T12:00:00Z'),
    );

    expect(range).toMatchObject({
      from: '2026-07-18',
      to: '2026-10-18',
      minimumDate: '2026-07-18',
      maximumDate: '2026-10-18',
      hasCustomFilters: false,
    });
  });

  it('clamps requested dates to the three-month window', () => {
    const range = resolveTransactionDateRange(
      '2020-01-01',
      '2026-12-31',
      new Date('2026-10-18T12:00:00Z'),
    );

    expect(range.from).toBe('2026-07-18');
    expect(range.to).toBe('2026-10-18');
    expect(range.hasCustomFilters).toBe(true);
  });

  it('clamps the start day to the last day when the target month is shorter', () => {
    const range = resolveTransactionDateRange(
      undefined,
      undefined,
      new Date('2026-05-31T12:00:00Z'),
    );

    expect(range.minimumDate).toBe('2026-02-28');
  });

  it('ignores malformed filter dates and keeps a valid calendar range', () => {
    const range = resolveTransactionDateRange(
      '2026-02-30',
      '2026-10-18',
      new Date('2026-10-18T12:00:00Z'),
    );

    expect(range.from).toBe('2026-07-18');
    expect(range.to).toBe('2026-10-18');
    expect(range.hasCustomFilters).toBe(true);
  });
});

describe('transaction month groups', () => {
  it('groups transactions by Johannesburg calendar month in input order', () => {
    const transactions: Transaction[] = [
      createTransaction('october', '2026-10-01T00:30:00Z'),
      createTransaction('september', '2026-09-28T12:00:00Z'),
      createTransaction('september-later', '2026-09-02T12:00:00Z'),
      createTransaction('august', '2026-08-31T12:00:00Z'),
    ];

    const groups = groupTransactionsByMonth(transactions);

    expect(groups.map((group) => [group.key, group.label, group.transactions.length])).toEqual([
      ['2026-10', 'October 2026', 1],
      ['2026-09', 'September 2026', 2],
      ['2026-08', 'August 2026', 1],
    ]);
  });
});

function createTransaction(transactionId: string, occurredAt: string): Transaction {
  return {
    transactionId,
    merchant: 'Synthetic merchant',
    description: 'Synthetic transaction',
    occurredAt,
    amountMinor: '1000',
    currencyCode: 'ZAR',
    state: 'Completed',
  };
}
