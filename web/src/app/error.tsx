'use client';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { StatePanel } from '@/components/ui/state-panel';
export default function ErrorPage({
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <StatePanel
      error
      title="We couldn’t load this page"
      action={
        <>
          <Button onClick={reset}>Try again</Button>
          <Link href="/login">Return to demo sign-in</Link>
        </>
      }
    >
      The service may be temporarily unavailable. Please try again in a moment. If you were
      submitting a dispute, check My disputes before starting a new one.
    </StatePanel>
  );
}
