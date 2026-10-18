import styles from '@/styles/page.module.css';
export default function Loading() {
  return (
    <div className={styles.panel}>
      <div className={styles.panelBody} role="status" aria-live="polite">
        Loading your portal…
      </div>
    </div>
  );
}
