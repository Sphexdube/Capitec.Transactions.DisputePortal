import Link from 'next/link';
import { StatePanel } from '@/components/ui/state-panel';
export default function NotFound() {
  return (
    <StatePanel
      title="This page isn’t available"
      action={<Link href="/">Back to transactions</Link>}
    >
      Check the address or return to your transaction history.
    </StatePanel>
  );
}
