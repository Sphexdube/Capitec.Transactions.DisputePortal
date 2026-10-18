import Link from 'next/link';
import { Icon } from '@/components/ui/icon';
import styles from '@/styles/page.module.css';
export function SupportCard() {
  return (
    <aside className={styles.support}>
      <Icon name="help" width="28" height="28" />
      <h3>
        We’ll help you
        <br />
        find your next step.
      </h3>
      <p>Unsure about a transaction? Check its details, then tell us what doesn’t look right.</p>
      <Link href="/help">
        How disputes work <Icon name="arrow" width="17" />
      </Link>
      <ol className={styles.steps}>
        <li>
          <span>
            <strong>Find the transaction</strong>Check the merchant, date and amount.
          </span>
        </li>
        <li>
          <span>
            <strong>Tell us what happened</strong>Choose a reason and add details.
          </span>
        </li>
        <li>
          <span>
            <strong>Follow its progress</strong>View updates in My disputes.
          </span>
        </li>
      </ol>
    </aside>
  );
}
