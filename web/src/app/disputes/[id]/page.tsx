import Link from 'next/link';
import { notFound } from 'next/navigation';
import type { Confirmation, Dispute, HistoryEvent } from '@/types/portal';
import { serverRequest } from '@/lib/api-server';
import { formatDate, formatMoney } from '@/lib/format';
import { STATUS_LABELS } from '@/constants/disputes';
import { StatusBadge } from '@/components/ui/status-badge';
import { Icon } from '@/components/ui/icon';
import { ConfirmationPanel } from '@/components/disputes/confirmation-panel';
import styles from '@/styles/page.module.css';
export default async function DisputePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  if (!/^[0-9a-f-]{36}$/i.test(id)) notFound();
  const [dispute, history, confirmation] = await Promise.all([
    serverRequest<Dispute>(`dispute/${id}`),
    serverRequest<HistoryEvent[]>(`dispute/${id}/history`),
    serverRequest<Confirmation>(`confirmation/${id}`),
  ]);
  return (
    <>
      <Link href="/disputes" className={styles.back}>
        <Icon name="arrow" width="16" />
        Back to my disputes
      </Link>
      <div className={styles.heading}>
        <p className={styles.eyebrow}>YOUR QUERY, AT A GLANCE</p>
        <h1>{dispute.transaction.merchant}</h1>
        <p>
          Submitted {formatDate(dispute.createdAt)} · <StatusBadge status={dispute.status} />
        </p>
      </div>
      <div className={styles.stack}>
        <section className={styles.panel}>
          <div className={styles.panelHeading}>
            <h2>Dispute details</h2>
            <span className={styles.amount}>
              {formatMoney(dispute.transaction.amountMinor, dispute.transaction.currencyCode)}
            </span>
          </div>
          <div className={styles.panelBody}>
            <dl className={styles.facts}>
              <div>
                <dt>Reason</dt>
                <dd>{dispute.reason}</dd>
              </div>
              <div>
                <dt>Transaction date</dt>
                <dd>{formatDate(dispute.transaction.occurredAt)}</dd>
              </div>
              <div>
                <dt>Additional details</dt>
                <dd>{dispute.explanation || 'No additional details provided.'}</dd>
              </div>
              <div>
                <dt>Dispute reference</dt>
                <dd>{dispute.disputeId}</dd>
              </div>
            </dl>
          </div>
        </section>
        <section className={styles.panel}>
          <div className={styles.panelHeading}>
            <h2>Progress so far</h2>
            <Icon name="clock" />
          </div>
          <div className={styles.panelBody}>
            <ol className={styles.timeline}>
              {history.map((event, index) => (
                <li key={`${event.occurredAt}-${index}`}>
                  <strong>{STATUS_LABELS[event.currentStatus] ?? event.currentStatus}</strong>
                  <p>
                    {formatDate(event.occurredAt)} · {event.actor}
                  </p>
                </li>
              ))}
            </ol>
          </div>
        </section>
        <ConfirmationPanel initial={confirmation} disputeId={id} />
      </div>
    </>
  );
}
