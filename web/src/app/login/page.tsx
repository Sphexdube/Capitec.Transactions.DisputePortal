import type { Metadata } from 'next';
import { serverRequest } from '@/lib/api-server';
import type { Customer } from '@/types/portal';
import { DemoSignIn } from '@/components/demo-sign-in/demo-sign-in';
import { SupportCard } from '@/components/support-card/support-card';
import { Icon } from '@/components/ui/icon';
import { StatePanel } from '@/components/ui/state-panel';
import styles from '@/styles/page.module.css';
export const metadata: Metadata = { title: 'Welcome' };
export default async function LoginPage() {
  const customers = await serverRequest<Customer[]>('demo-session', true);
  return (
    <>
      <div className={styles.heading}>
        <p className={styles.eyebrow}>LET’S GET YOU SORTED</p>
        <h1>
          Your transactions.
          <br />A clearer picture.
        </h1>
        <p>A simple way to query a transaction and keep track of what happens next.</p>
      </div>
      <section className={styles.hero}>
        <div>
          <h2>A question about a transaction?</h2>
          <p>Find it. Tell us what happened. Follow every update in one place.</p>
        </div>
        <span className={styles.heroIcon}>
          <Icon name="shield" width="42" height="42" />
        </span>
      </section>
      <div className={styles.contentGrid}>
        <section className={styles.panel}>
          <div className={styles.panelHeading}>
            <div>
              <h2>Welcome to your dispute portal</h2>
              <p>Choose a demo customer to explore the experience.</p>
            </div>
          </div>
          <div className={styles.panelBody}>
            {customers.length ? (
              <DemoSignIn customers={customers} />
            ) : (
              <StatePanel title="No demo customers available">
                The demo needs customer data before you can start a session.
              </StatePanel>
            )}
          </div>
        </section>
        <SupportCard />
      </div>
    </>
  );
}
