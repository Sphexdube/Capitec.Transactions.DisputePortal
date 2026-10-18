import { describe, expect, it } from 'vitest';
import { isSameOrigin } from '../src/lib/request-origin';
describe('same-origin check', () => {
  it('accepts the host the browser addressed', () => {
    expect(isSameOrigin('http://localhost:8082', 'localhost:8082', null)).toBe(true);
    expect(isSameOrigin('https://portal.example', 'web:3000', 'portal.example')).toBe(true);
    expect(isSameOrigin('https://portal.example', 'web:3000', 'portal.example, proxy')).toBe(true);
  });
  it('rejects other, missing or malformed origins', () => {
    expect(isSameOrigin('https://untrusted.example', 'localhost:8082', null)).toBe(false);
    expect(isSameOrigin(null, 'localhost:8082', null)).toBe(false);
    expect(isSameOrigin('http://localhost:8082', null, null)).toBe(false);
    expect(isSameOrigin('not a url', 'localhost:8082', null)).toBe(false);
  });
});
