import type { Metadata } from 'next';
import type { Page, Transaction } from '@/types/portal';
import { serverRequest, signedInCustomerName } from '@/lib/api-server';
import { TransactionList } from '@/components/transactions/transaction-list';
import { DateFilter } from '@/components/transactions/date-filter';
import { SupportCard } from '@/components/support-card/support-card';
import { Icon } from '@/components/ui/icon';
import { resolveTransactionDateRange } from '@/lib/transaction-history';
import styles from '@/styles/page.module.css';
export const metadata: Metadata = { title: 'Transactions' };
export default async function TransactionsPage({
  searchParams,
}: {
  searchParams: Promise<{ from?: string | string[]; to?: string | string[] }>;
}) {
  const filters = await searchParams;
  const dateRange = resolveTransactionDateRange(filters.from, filters.to);
  const query = new URLSearchParams({
    pageSize: '10',
    from: `${dateRange.from}T00:00:00+02:00`,
    to: `${dateRange.to}T23:59:59.9999999+02:00`,
  });
  const transactions = await serverRequest<Page<Transaction>>(`transaction?${query}`);
  const customerName = await signedInCustomerName();
  return (
    <>
      <div className={styles.heading}>
        <p className={styles.eyebrow}>YOUR MONEY, MADE CLEAR</p>
        <h1>Transactions</h1>
        <p>
          {customerName ? `Hi ${customerName.split(' ')[0]}, these are your transactions. ` : ''}
          Something doesn’t look right? Find a transaction and we’ll help you take the next step.
        </p>
      </div>
      <section className={styles.hero}>
        <div>
          <h2>Let’s get to the bottom of it.</h2>
          <p>Raise a query in a few simple steps and keep track of every update in My disputes.</p>
        </div>
        <span className={styles.heroIcon}>
          <Icon name="disputes" width="40" height="40" />
        </span>
      </section>
      <DateFilter {...dateRange} />
      <div className={styles.contentGrid}>
        <TransactionList
          key={query.toString()}
          initialPage={transactions}
          query={query.toString()}
        />
        <SupportCard />
      </div>
    </>
  );
}
