import Link from 'next/link';
import { Button } from '@/components/ui/button';
import styles from './date-filter.module.css';
export function DateFilter({
  from,
  to,
  minimumDate,
  maximumDate,
  hasCustomFilters,
}: {
  from: string;
  to: string;
  minimumDate: string;
  maximumDate: string;
  hasCustomFilters: boolean;
}) {
  return (
    <div>
      <form className={styles.filters} action="/" method="get">
        <label htmlFor="from">
          From date
          <input type="date" id="from" name="from" defaultValue={from} min={minimumDate} max={to} />
        </label>
        <label htmlFor="to">
          To date
          <input type="date" id="to" name="to" defaultValue={to} min={from} max={maximumDate} />
        </label>
        <Button type="submit" variant="secondary">
          Apply filters
        </Button>
        {hasCustomFilters && <Link href="/">Clear date filters</Link>}
      </form>
      <p className={styles.rangeNote}>
        Transaction history is available for the last three months.
      </p>
    </div>
  );
}
