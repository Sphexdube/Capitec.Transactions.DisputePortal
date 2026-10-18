import { afterEach, describe, expect, it, vi } from 'vitest';
import { request } from '../src/lib/api-client';
afterEach(() => vi.unstubAllGlobals());
describe('customer-facing API failures', () => {
  it('preserves safe validation messages instead of reporting success', async () => {
    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValue(
          Response.json({ errors: [{ message: 'A dispute already exists.' }] }, { status: 409 }),
        ),
    );
    await expect(request('transaction')).rejects.toThrow('A dispute already exists.');
  });
  it('unwraps successful API data', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(Response.json({ data: { items: [] } })));
    await expect(request('transaction')).resolves.toEqual({ items: [] });
  });
});
