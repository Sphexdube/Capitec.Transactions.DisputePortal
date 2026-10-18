import { STATUS_LABELS } from '@/constants/disputes';
import styles from './ui.module.css';
export function StatusBadge({ status }: { status: string }) {
  const tone =
    status === 'Resolved' || status === 'Recorded'
      ? 'success'
      : status === 'Rejected'
        ? 'danger'
        : status === 'UnderReview' || status === 'Pending'
          ? 'warning'
          : 'info';
  return (
    <span className={`${styles.badge} ${styles[tone]}`}>
      <span className={styles.dot} />
      {STATUS_LABELS[status] ?? status}
    </span>
  );
}
