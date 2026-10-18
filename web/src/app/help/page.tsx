import Link from 'next/link';
import styles from '@/styles/page.module.css';
const questions = [
  [
    'How do I dispute a transaction?',
    'Open Transactions, find the payment and select its arrow. Choose a reason, add any useful details and submit. Your query will then appear in My disputes.',
  ],
  [
    'What happens after I submit?',
    'The portal records your dispute as Submitted. Open it from My disputes to see its recorded history and simulated in-app confirmation.',
  ],
  [
    'What if my confirmation is pending?',
    'Your dispute has already been accepted. Confirmation is processed separately. Open your dispute and select Check for confirmation to see the latest result.',
  ],
  [
    'Can I use my real banking information?',
    'No. This is an independent assessment using synthetic customers and transactions. Do not enter real account details, passwords, card numbers or PINs.',
  ],
  [
    'Does this send a dispute to Capitec?',
    'No. This project is not connected to Capitec Bank and cannot investigate or reverse a real payment. For a real banking query, use the official Capitec app or website.',
  ],
];
export default function HelpPage() {
  return (
    <>
      <div className={styles.heading}>
        <p className={styles.eyebrow}>A LITTLE HELP GOES A LONG WAY</p>
        <h1>Help & support</h1>
        <p>Everything you need to get started with this demo.</p>
      </div>
      <div className={styles.stack}>
        {questions.map(([title, copy]) => (
          <section className={styles.panel} key={title}>
            <div className={styles.panelBody}>
              <h2>{title}</h2>
              <p className={styles.helpCopy}>{copy}</p>
            </div>
          </section>
        ))}
        <Link href="/">Back to transactions →</Link>
      </div>
    </>
  );
}
