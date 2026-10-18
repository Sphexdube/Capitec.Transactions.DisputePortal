import type { ReactNode } from 'react';
import { Icon } from './icon';
import styles from './ui.module.css';
export function StatePanel({
  title,
  children,
  action,
  error = false,
}: {
  title: string;
  children: ReactNode;
  action?: ReactNode;
  error?: boolean;
}) {
  return (
    <section className={styles.state} role={error ? 'alert' : undefined}>
      <span className={`${styles.stateIcon} ${error ? styles.danger : styles.info}`}>
        <Icon name={error ? 'alert' : 'transactions'} width="36" height="36" />
      </span>
      <h2>{title}</h2>
      <div className={styles.stateCopy}>{children}</div>
      {action}
    </section>
  );
}
