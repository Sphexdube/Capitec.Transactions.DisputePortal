'use client';
import { useState } from 'react';
import type { Confirmation } from '@/types/portal';
import { request } from '@/lib/api-client';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/ui/status-badge';
import styles from '@/styles/page.module.css';
export function ConfirmationPanel({
  initial,
  disputeId,
}: {
  initial: Confirmation;
  disputeId: string;
}) {
  const [confirmation, setConfirmation] = useState(initial);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  async function refresh() {
    setBusy(true);
    setError('');
    try {
      setConfirmation(await request<Confirmation>(`confirmation/${disputeId}`));
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Could not check confirmation.');
    } finally {
      setBusy(false);
    }
  }
  return (
    <section className={styles.panel}>
      <div className={styles.panelHeading}>
        <h2>Submission confirmation</h2>
        <StatusBadge status={confirmation.status} />
      </div>
      <div className={`${styles.panelBody} ${styles.stack}`}>
        <p aria-live="polite">
          {confirmation.status === 'Recorded'
            ? confirmation.message || 'Your submission confirmation has been recorded.'
            : 'Your dispute is accepted. The simulated confirmation is still being processed.'}
        </p>
        {confirmation.status !== 'Recorded' && (
          <Button variant="secondary" disabled={busy} onClick={refresh}>
            {busy ? 'Checking…' : 'Check for confirmation'}
          </Button>
        )}
        {error && (
          <div role="alert" className={styles.error}>
            {error}
          </div>
        )}
        <p className={styles.note}>This is an in-app demo confirmation. No email or SMS is sent.</p>
      </div>
    </section>
  );
}
