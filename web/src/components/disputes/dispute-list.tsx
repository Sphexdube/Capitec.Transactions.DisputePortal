'use client';
import { useState } from 'react';
import Link from 'next/link';
import type { Dispute, Page } from '@/types/portal';
import { request } from '@/lib/api-client';
import { formatDate, formatMoney } from '@/lib/format';
import { Button } from '@/components/ui/button';
import { Icon } from '@/components/ui/icon';
import { StatusBadge } from '@/components/ui/status-badge';
import { StatePanel } from '@/components/ui/state-panel';
import styles from './dispute-list.module.css';
import page from '@/styles/page.module.css';
export function DisputeList({ initialPage }: { initialPage: Page<Dispute> }) {
  const [items, setItems] = useState(initialPage.items);
  const [cursor, setCursor] = useState(initialPage.nextCursor);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  async function loadMore() {
    setBusy(true);
    setError('');
    try {
      const next = await request<Page<Dispute>>(
        `dispute?pageSize=10&cursor=${encodeURIComponent(cursor!)}`,
      );
      setItems((previous) => [...previous, ...next.items]);
      setCursor(next.nextCursor);
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Could not load more disputes.');
    } finally {
      setBusy(false);
    }
  }
  return (
    <section className={page.panel}>
      <div className={page.panelHeading}>
        <div>
          <h2>Your dispute history</h2>
          <p>Current and previous queries, newest first.</p>
        </div>
        <Icon name="disputes" />
      </div>
      {!items.length ? (
        <StatePanel
          title="You’re all caught up"
          action={
            <Link href="/">
              View transactions <Icon name="arrow" width="16" />
            </Link>
          }
        >
          You haven’t submitted any disputes yet. If something doesn’t look right, start with your
          transactions.
        </StatePanel>
      ) : (
        <>
          <ul className={styles.list}>
            {items.map((dispute) => (
              <li key={dispute.disputeId}>
                <Link className={styles.row} href={`/disputes/${dispute.disputeId}`}>
                  <span className={styles.icon}>
                    <Icon name="disputes" />
                  </span>
                  <div className={styles.copy}>
                    <strong>{dispute.transaction.merchant}</strong>
                    <span>{dispute.reason}</span>
                    <small>Submitted {formatDate(dispute.createdAt)}</small>
                  </div>
                  <div className={styles.summary}>
                    <strong>
                      {formatMoney(
                        dispute.transaction.amountMinor,
                        dispute.transaction.currencyCode,
                      )}
                    </strong>
                    <StatusBadge status={dispute.status} />
                  </div>
                  <Icon name="chevron" width="16" />
                </Link>
              </li>
            ))}
          </ul>
          <div className={styles.footer}>
            <span>{items.length} disputes loaded</span>
            {cursor && (
              <Button variant="secondary" onClick={loadMore} disabled={busy}>
                {busy ? 'Loading…' : 'Load more'}
              </Button>
            )}
          </div>
          {error && (
            <div className={page.panelBody}>
              <div role="alert" className={page.error}>
                {error} Use Load more to retry.
              </div>
            </div>
          )}
        </>
      )}
    </section>
  );
}
