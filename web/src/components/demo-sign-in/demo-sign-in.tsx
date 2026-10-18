'use client';
import { useState, type FormEvent } from 'react';
import { useRouter } from 'next/navigation';
import type { Customer } from '@/types/portal';
import { request } from '@/lib/api-client';
import { Button } from '@/components/ui/button';
import { Icon } from '@/components/ui/icon';
import styles from '@/styles/page.module.css';
export function DemoSignIn({ customers }: { customers: Customer[] }) {
  const [customerId, setCustomerId] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const router = useRouter();
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    setError('');
    try {
      await request('demo-session', {
        method: 'POST',
        body: JSON.stringify({ customerId: Number(customerId) }),
      });
      router.push('/');
      router.refresh();
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Could not start your session.');
    } finally {
      setBusy(false);
    }
  }
  return (
    <form className={styles.form} onSubmit={submit}>
      <label htmlFor="customer">
        Demo customer
        <select
          id="customer"
          required
          value={customerId}
          onChange={(event) => setCustomerId(event.target.value)}
          disabled={busy}
        >
          <option value="">Choose a customer</option>
          {customers.map((customer) => (
            <option key={customer.customerId} value={customer.customerId}>
              {customer.displayName}
            </option>
          ))}
        </select>
      </label>
      {error && (
        <div role="alert" className={styles.error}>
          {error}
        </div>
      )}
      <Button type="submit" disabled={busy || !customers.length}>
        {busy ? 'Starting your session…' : 'Continue to transactions'}
        <Icon name="arrow" width="18" />
      </Button>
      <p className={styles.note}>
        No banking credentials needed. All customers and transactions in this assessment are
        synthetic.
      </p>
    </form>
  );
}
