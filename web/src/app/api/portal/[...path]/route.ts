import { NextRequest, NextResponse } from 'next/server';
import { apiBaseUrl } from '@/lib/api-server';
import { CUSTOMER_NAME_COOKIE, SESSION_COOKIE } from '@/lib/session';
import { allowedApiRoute } from '@/lib/api-routes';
import { isSameOrigin } from '@/lib/request-origin';

const error = (message: string, status: number) =>
  NextResponse.json({ errors: [{ message }] }, { status });
async function handler(request: NextRequest, context: { params: Promise<{ path: string[] }> }) {
  const path = (await context.params).path.join('/');
  if (!allowedApiRoute(request.method, path)) return error('This action is not available.', 404);
  const mutation = request.method !== 'GET';
  const sameOrigin = isSameOrigin(
    request.headers.get('origin'),
    request.headers.get('host'),
    request.headers.get('x-forwarded-host'),
  );
  if (mutation && !sameOrigin) return error('Please refresh the page before trying again.', 403);
  if (path === 'demo-session' && request.method === 'DELETE') {
    const response = NextResponse.json({ data: null });
    response.cookies.delete(SESSION_COOKIE);
    response.cookies.delete(CUSTOMER_NAME_COOKIE);
    return response;
  }
  const token = request.cookies.get(SESSION_COOKIE)?.value;
  if (path !== 'demo-session' && !token)
    return error('Your session has ended. Please sign in again.', 401);
  const headers = new Headers({ 'Content-Type': 'application/json' });
  if (token && path !== 'demo-session') headers.set('Authorization', `Bearer ${token}`);
  const key = request.headers.get('idempotency-key');
  if (key) headers.set('Idempotency-Key', key);
  try {
    const upstream = await fetch(`${apiBaseUrl()}/api/v1/${path}${request.nextUrl.search}`, {
      method: request.method,
      headers,
      body: mutation ? await request.text() : undefined,
      cache: 'no-store',
      signal: AbortSignal.timeout(15000),
      redirect: 'error',
    });
    const body = await upstream.json();
    if (upstream.status >= 500)
      console.error(`API returned ${upstream.status}: ${request.method} ${path}`);
    if (path === 'demo-session' && request.method === 'POST' && upstream.ok) {
      const response = NextResponse.json({ data: { signedIn: true } });
      const cookie = {
        httpOnly: true,
        sameSite: 'lax',
        secure:
          process.env.SESSION_COOKIE_SECURE !== 'false' && request.nextUrl.protocol === 'https:',
        path: '/',
        expires: new Date(body.data.expiresAt),
      } as const;
      response.cookies.set(SESSION_COOKIE, body.data.accessToken, cookie);
      response.cookies.set(CUSTOMER_NAME_COOKIE, body.data.displayName, cookie);
      return response;
    }
    return NextResponse.json(body, {
      status: upstream.status,
      headers: { 'Cache-Control': 'no-store' },
    });
  } catch (failure) {
    console.error(`API request failed: ${request.method} ${path}`, failure);
    return error('The service is temporarily unavailable. Please try again.', 503);
  }
}
export { handler as GET, handler as POST, handler as DELETE };
