import type { Envelope } from '@/types/portal';
export async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`/api/portal/${path}`, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  });
  const body = await response.json();
  if (!response.ok) {
    throw new Error(
      body.errors?.map((error: { message: string }) => error.message).join(' ') ||
        'We could not complete your request. Please try again.',
    );
  }
  return (body as Envelope<T>).data;
}
