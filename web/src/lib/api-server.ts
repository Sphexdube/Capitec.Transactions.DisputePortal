import 'server-only';
import { cookies } from 'next/headers';
import { notFound, redirect } from 'next/navigation';
import type { Envelope } from '@/types/portal';
import { CUSTOMER_NAME_COOKIE, SESSION_COOKIE } from './session';
export function apiBaseUrl(): string {
  const configured = process.env.API_BASE_URL;
  if (!configured && process.env.NODE_ENV === 'production')
    throw new Error('API_BASE_URL is not configured.');
  return (configured || 'http://localhost:5147').replace(/\/$/, '');
}
export async function signedInCustomerName(): Promise<string | null> {
  const store = await cookies();
  if (!store.get(SESSION_COOKIE)) return null;
  return store.get(CUSTOMER_NAME_COOKIE)?.value || null;
}
export async function serverRequest<T>(path: string, anonymous = false): Promise<T> {
  const token = (await cookies()).get(SESSION_COOKIE)?.value;
  if (!anonymous && !token) redirect('/login');
  let response: Response;
  try {
    response = await fetch(`${apiBaseUrl()}/api/v1/${path}`, {
      cache: 'no-store',
      headers: token && !anonymous ? { Authorization: `Bearer ${token}` } : {},
      signal: AbortSignal.timeout(10000),
    });
  } catch (failure) {
    console.error(`API request failed: GET ${path}`, failure);
    throw new Error('The service is temporarily unavailable. Please try again.');
  }
  if (response.status === 401) redirect('/login');
  if (response.status === 404) notFound();
  if (!response.ok) {
    console.error(`API request returned ${response.status}: GET ${path}`);
    throw new Error('We could not load this information. Please try again.');
  }
  return ((await response.json()) as Envelope<T>).data;
}
