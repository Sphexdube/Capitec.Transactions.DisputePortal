import { describe, expect, it } from 'vitest';
import { formatMoney, formatDate } from '../src/lib/format';
const compact = (value: string) => value.replace(/\s/g, '');
describe('minor-unit currency formatting', () => {
  it('keeps values above the safe integer limit exact', () => {
    expect(compact(formatMoney('9223372036854775807', 'ZAR'))).toContain('92233720368547758,07');
  });
  it('preserves negative sub-unit values', () => {
    expect(compact(formatMoney('-1', 'ZAR'))).toBe('-R0,01');
  });
  it('uses the currency precision for zero and three-decimal currencies', () => {
    expect(compact(formatMoney('1250', 'JPY'))).toContain('1250');
    expect(compact(formatMoney('12345', 'KWD'))).toContain('12,345');
  });
  it('renders dates in South African time across a UTC date boundary', () => {
    expect(formatDate('2026-10-17T23:30:00Z')).toContain('18');
  });
});
