'use client';
import { useState } from 'react';
import Link from 'next/link';
import type { Page, Transaction } from '@/types/portal';
import { formatDate, formatMoney } from '@/lib/format';
import { groupTransactionsByMonth } from '@/lib/transaction-history';
import { request } from '@/lib/api-client';
import { Button } from '@/components/ui/button';
import { Icon } from '@/components/ui/icon';
import { StatusBadge } from '@/components/ui/status-badge';
import { StatePanel } from '@/components/ui/state-panel';
import { DisputeForm } from '@/components/disputes/dispute-form';
import styles from './transaction-list.module.css';
import page from '@/styles/page.module.css';

export function TransactionList({
  initialPage,
  query,
}: {
  initialPage: Page<Transaction>;
  query: string;
}) {
  const [items, setItems] = useState(initialPage.items);
  const [cursor, setCursor] = useState(initialPage.nextCursor);
  const [selected, setSelected] = useState<Transaction | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const monthGroups = groupTransactionsByMonth(items);
  async function loadMore() {
    setBusy(true);
    setError('');
    try {
      const params = new URLSearchParams(query);
      params.set('cursor', cursor!);
      const next = await request<Page<Transaction>>(`transaction?${params}`);
      setItems((previous) => [...previous, ...next.items]);
      setCursor(next.nextCursor);
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Could not load more transactions.');
    } finally {
      setBusy(false);
    }
  }
  if (selected) return <DisputeForm transaction={selected} onBack={() => setSelected(null)} />;
  return (
    <section className={page.panel}>
      <div className={page.panelHeading}>
        <div>
          <h2>Transaction history</h2>
          <p>Choose a transaction to view its details or raise a dispute.</p>
        </div>
        <Icon name="transactions" />
      </div>
      {!items.length ? (
        <StatePanel title="No transactions found" action={<Link href="/">Clear date filters</Link>}>
          Try a different date range to find the transaction you’re looking for.
        </StatePanel>
      ) : (
        <>
          <div className={styles.monthGroups}>
            {monthGroups.map((group) => (
              <details className={styles.monthGroup} key={group.key}>
                <summary className={styles.monthSummary}>
                  <span className={styles.monthName}>{group.label}</span>
                  <span className={styles.monthCount}>
                    {group.transactions.length}{' '}
                    {group.transactions.length === 1 ? 'transaction' : 'transactions'}
                  </span>
                  <span aria-hidden="true" className={styles.monthChevron}>
                    <Icon name="chevron" width="18" />
                  </span>
                </summary>
                <div className={styles.tableWrap}>
                  <table className={styles.table}>
                    <caption className={styles.visuallyHidden}>
                      {group.label} transactions, newest first
                    </caption>
                    <thead>
                      <tr>
                        <th scope="col">Transaction</th>
                        <th scope="col">Status</th>
                        <th scope="col" className={styles.right}>
                          Amount
                        </th>
                        <th scope="col">
                          <span className={styles.visuallyHidden}>Action</span>
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {group.transactions.map((transaction) => (
                        <tr key={transaction.transactionId}>
                          <td>
                            <div className={styles.merchant}>
                              <span className={styles.merchantIcon}>
                                <Icon name="card" />
                              </span>
                              <div>
                                <strong>{transaction.merchant}</strong>
                                <span>{formatDate(transaction.occurredAt)}</span>
                              </div>
                            </div>
                          </td>
                          <td className={styles.status}>
                            <StatusBadge status={transaction.state} />
                          </td>
                          <td className={styles.money}>
                            {formatMoney(transaction.amountMinor, transaction.currencyCode)}
                          </td>
                          <td className={styles.action}>
                            <button
                              aria-label={`View ${transaction.merchant} transaction on ${formatDate(transaction.occurredAt)}`}
                              onClick={() => setSelected(transaction)}
                            >
                              <Icon name="chevron" width="18" />
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </details>
            ))}
          </div>
          <div className={styles.listFooter}>
            <span>{items.length} transactions loaded</span>
            {cursor && (
              <Button variant="secondary" disabled={busy} onClick={loadMore}>
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
