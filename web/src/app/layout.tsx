import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { PortalShell } from '@/components/portal-shell/portal-shell';
import { signedInCustomerName } from '@/lib/api-server';
import './globals.css';

export const metadata: Metadata = {
  title: { default: 'Transaction disputes | Capitec assessment', template: '%s | Dispute portal' },
  description:
    'Independent assessment demo. View synthetic transactions and track disputes. Not a Capitec Bank service.',
};

export default async function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en-ZA">
      <body>
        <PortalShell customerName={await signedInCustomerName()}>{children}</PortalShell>
      </body>
    </html>
  );
}
