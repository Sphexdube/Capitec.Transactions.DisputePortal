'use client';
import { useEffect, useRef, useState, type FormEvent } from 'react';
import Link from 'next/link';
import type { SubmittedDispute, Transaction } from '@/types/portal';
import { formatDate, formatMoney } from '@/lib/format';
import { request } from '@/lib/api-client';
import { DISPUTE_REASONS, EXPLANATION_MAX_LENGTH } from '@/constants/disputes';
import { Button } from '@/components/ui/button';
import { Icon } from '@/components/ui/icon';
import { StatePanel } from '@/components/ui/state-panel';
import styles from '@/styles/page.module.css';

export function DisputeForm({
  transaction,
  onBack,
}: {
  transaction: Transaction;
  onBack: () => void;
}) {
  const heading = useRef<HTMLHeadingElement>(null);
  useEffect(() => {
    heading.current?.focus();
  }, []);
  const [reason, setReason] = useState('');
  const [explanation, setExplanation] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [accepted, setAccepted] = useState<SubmittedDispute | null>(null);
  // Preserve the key for an unchanged request so a timeout can be retried safely.
  const attempt = useRef<{ payload: string; key: string } | null>(null);
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    setBusy(true);
    setError('');
    const payload = JSON.stringify({ reason, explanation: explanation.trim() || null });
    if (attempt.current?.payload !== payload)
      attempt.current = { payload, key: crypto.randomUUID() };
    try {
      setAccepted(
        await request<SubmittedDispute>(`transaction/${transaction.transactionId}/dispute`, {
          method: 'POST',
          body: payload,
          headers: { 'Idempotency-Key': attempt.current.key },
        }),
      );
    } catch (error) {
      setError(
        error instanceof Error ? error.message : 'Could not submit your dispute. Please try again.',
      );
    } finally {
      setBusy(false);
    }
  }
  if (accepted)
    return (
      <section className={styles.panel}>
        <StatePanel
          title="Your dispute has been submitted"
          action={
            <Link href={`/disputes/${accepted.disputeId}`}>
              View dispute and confirmation <Icon name="arrow" width="16" />
            </Link>
          }
        >
          We’ve recorded your query about {transaction.merchant}. You can follow its status in My
          disputes. Your simulated confirmation will appear once processed.
        </StatePanel>
      </section>
    );
  return (
    <section className={styles.panel}>
      <div className={styles.panelHeading}>
        <div>
          <h2 ref={heading} tabIndex={-1}>
            Let’s look into this transaction
          </h2>
          <p>Check the details below before submitting your dispute.</p>
        </div>
      </div>
      <div className={`${styles.panelBody} ${styles.stack}`}>
        <dl className={styles.facts}>
          <div>
            <dt>Merchant</dt>
            <dd>{transaction.merchant}</dd>
          </div>
          <div>
            <dt>Amount</dt>
            <dd>{formatMoney(transaction.amountMinor, transaction.currencyCode)}</dd>
          </div>
          <div>
            <dt>Date</dt>
            <dd>{formatDate(transaction.occurredAt)}</dd>
          </div>
          <div>
            <dt>Description</dt>
            <dd>{transaction.description}</dd>
          </div>
        </dl>
        <form className={styles.form} onSubmit={submit}>
          <label htmlFor="reason">
            What went wrong?
            <select
              id="reason"
              required
              value={reason}
              disabled={busy}
              onChange={(event) => setReason(event.target.value)}
            >
              <option value="">Choose a reason</option>
              {DISPUTE_REASONS.map((value) => (
                <option key={value}>{value}</option>
              ))}
            </select>
          </label>
          <label htmlFor="explanation">
            Anything else we should know?{' '}
            <small>Optional. Please don’t include card numbers, PINs or passwords.</small>
            <textarea
              id="explanation"
              rows={4}
              maxLength={EXPLANATION_MAX_LENGTH}
              value={explanation}
              disabled={busy}
              onChange={(event) => setExplanation(event.target.value)}
              aria-describedby="explanation-count"
              placeholder="Tell us a little more about what happened…"
            />
            <small id="explanation-count">
              {explanation.length} / {EXPLANATION_MAX_LENGTH} characters
            </small>
          </label>
          {error && (
            <div role="alert" className={styles.error}>
              {error}
              <p>
                <Link href="/disputes">Check existing disputes</Link> or retry your submission.
              </p>
            </div>
          )}
          <div className={styles.actions}>
            <Button variant="secondary" disabled={busy} onClick={onBack}>
              Back to transactions
            </Button>
            <Button type="submit" disabled={busy}>
              {busy ? 'Submitting…' : 'Submit dispute'}
              <Icon name="arrow" width="18" />
            </Button>
          </div>
        </form>
      </div>
    </section>
  );
}
