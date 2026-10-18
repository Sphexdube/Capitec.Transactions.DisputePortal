'use client';
import Image from 'next/image';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useState, type ReactNode } from 'react';
import { Icon } from '@/components/ui/icon';
import { request } from '@/lib/api-client';
import styles from './portal-shell.module.css';

const navigation = [
  { href: '/', label: 'Transactions', icon: 'transactions' },
  { href: '/disputes', label: 'My disputes', icon: 'disputes' },
  { href: '/help', label: 'Help & support', icon: 'help' },
] as const;
export function PortalShell({
  children,
  customerName,
}: {
  children: ReactNode;
  customerName: string | null;
}) {
  const pathname = usePathname();
  const router = useRouter();
  const [signingOut, setSigningOut] = useState(false);
  const [error, setError] = useState('');
  // Signed-out prefetches of protected pages cache the redirect to sign-in and would replay it after sign-in.
  const prefetch = pathname === '/login' ? false : undefined;
  async function signOut() {
    setSigningOut(true);
    setError('');
    try {
      await request('demo-session', { method: 'DELETE' });
      router.push('/login');
      router.refresh();
    } catch {
      setError('Could not sign out. Please try again.');
    } finally {
      setSigningOut(false);
    }
  }
  return (
    <div className={styles.root}>
      <a className={styles.skip} href="#main-content">
        Skip to content
      </a>
      <header className={styles.header}>
        <Link
          href="/"
          prefetch={prefetch}
          className={styles.brand}
          aria-label="Capitec assessment home"
        >
          <Image src="/brand/capitec-symbol.svg" width={43} height={27} alt="" priority />
          <span>Capitec</span>
        </Link>
        <div className={styles.product}>
          <span className={styles.divider} />
          Transaction dispute portal
        </div>
        <span className={styles.demo}>
          <span />
          Assessment demo
        </span>
        {customerName && pathname !== '/login' && (
          <span className={styles.customer}>
            <Icon name="user" />
            <span>
              <small>Signed in as</small>
              {customerName}
            </span>
          </span>
        )}
        {pathname !== '/login' && (
          <button
            className={styles.mobileSignOut}
            aria-label="Leave demo session"
            disabled={signingOut}
            onClick={signOut}
          >
            <Icon name="logout" />
          </button>
        )}
      </header>
      <div className={styles.body}>
        <aside className={styles.sidebar}>
          <p className={styles.navLabel}>YOUR PORTAL</p>
          <nav aria-label="Main navigation" className={styles.nav}>
            {navigation.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                prefetch={prefetch}
                className={`${styles.navItem} ${(item.href === '/' ? pathname === '/' : pathname.startsWith(item.href)) ? styles.active : ''}`}
                aria-current={
                  (item.href === '/' ? pathname === '/' : pathname.startsWith(item.href))
                    ? 'page'
                    : undefined
                }
              >
                <Icon name={item.icon} />
                {item.label}
              </Link>
            ))}
          </nav>
          <div className={styles.sidebarBottom}>
            <div className={styles.assurance}>
              <Icon name="shield" />
              <strong>
                A little clarity.
                <br />A lot of peace of mind.
              </strong>
              <p>Track your transaction queries, all in one place.</p>
            </div>
            {pathname !== '/login' && (
              <button className={styles.signOut} disabled={signingOut} onClick={signOut}>
                <Icon name="logout" />
                {signingOut ? 'Signing out…' : 'Leave demo session'}
              </button>
            )}
            {error && <p role="alert">{error}</p>}
          </div>
        </aside>
        <div className={styles.workspace}>
          <main id="main-content" className={styles.main} tabIndex={-1}>
            {children}
          </main>
          <footer className={styles.footer}>
            <span>Independent assessment · Synthetic data only</span>
            <span>Not a Capitec Bank service or endorsed by Capitec.</span>
          </footer>
        </div>
      </div>
    </div>
  );
}
