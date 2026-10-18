import { describe, expect, it } from 'vitest';
import { allowedApiRoute } from '../src/lib/api-routes';
const id = 'd54602c3-8d6f-4071-a70f-684247939fe9';
describe('BFF endpoint allowlist', () => {
  it('permits the customer journey', () => {
    expect(allowedApiRoute('POST', 'demo-session')).toBe(true);
    expect(allowedApiRoute('GET', 'transaction')).toBe(true);
    expect(allowedApiRoute('POST', `transaction/${id}/dispute`)).toBe(true);
    expect(allowedApiRoute('GET', `dispute/${id}/history`)).toBe(true);
    expect(allowedApiRoute('GET', `confirmation/${id}`)).toBe(true);
  });
  it('does not expose arbitrary routes, traversal or customer status mutations', () => {
    for (const path of [
      '../admin',
      'demo-session/extra',
      'transaction/../../admin',
      `dispute/${id}/status`,
      'https://example.com',
    ]) {
      expect(allowedApiRoute('GET', path)).toBe(false);
      expect(allowedApiRoute('POST', path)).toBe(false);
    }
    expect(allowedApiRoute('DELETE', `dispute/${id}`)).toBe(false);
  });
});
