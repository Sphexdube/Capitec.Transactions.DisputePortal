import type { Metadata } from 'next';
import type { Dispute, Page } from '@/types/portal';
import { serverRequest } from '@/lib/api-server';
import { DisputeList } from '@/components/disputes/dispute-list';
import { SupportCard } from '@/components/support-card/support-card';
import styles from '@/styles/page.module.css';
export const metadata: Metadata = { title: 'My disputes' };
export default async function DisputesPage() {
  const disputes = await serverRequest<Page<Dispute>>('dispute?pageSize=10');
  return (
    <>
      <div className={styles.heading}>
        <p className={styles.eyebrow}>WITH YOU, EVERY STEP</p>
        <h1>My disputes</h1>
        <p>A clear view of your queries, their progress and what happens next.</p>
      </div>
      <div className={styles.contentGrid}>
        <DisputeList initialPage={disputes} />
        <SupportCard />
      </div>
    </>
  );
}
