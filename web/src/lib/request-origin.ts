/** Compare the browser Origin with the host the browser addressed, so the check holds behind port mapping and proxies. */
export function isSameOrigin(
  origin: string | null,
  host: string | null,
  forwardedHost: string | null,
): boolean {
  const expectedHost = forwardedHost?.split(',')[0].trim() || host;
  if (!origin || !expectedHost) return false;
  try {
    return new URL(origin).host === expectedHost;
  } catch {
    return false;
  }
}
