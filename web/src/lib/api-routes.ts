const resourceId = '[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}';
export function allowedApiRoute(method: string, path: string): boolean {
  if (path === 'demo-session') return method === 'GET' || method === 'POST' || method === 'DELETE';
  if (method === 'GET')
    return (
      /^(transaction|dispute)$/.test(path) ||
      new RegExp(`^(dispute/${resourceId}(/history)?|confirmation/${resourceId})$`).test(path)
    );
  return method === 'POST' && new RegExp(`^transaction/${resourceId}/dispute$`).test(path);
}
