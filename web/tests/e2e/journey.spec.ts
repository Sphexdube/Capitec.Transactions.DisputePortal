import { expect, test } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import type { Dispute, Transaction } from '../../src/types/portal';

test('customer can find, dispute and track a synthetic transaction', async ({ page }, testInfo) => {
  await page.goto('/');
  await expect(page).toHaveURL(/\/login$/);
  await page.screenshot({ path: testInfo.outputPath('sign-in.png'), fullPage: true });
  await page.getByLabel('Demo customer').selectOption({ index: 1 });
  await page.getByRole('button', { name: 'Continue to transactions' }).click();
  await expect(page.getByRole('heading', { name: 'Transactions', exact: true })).toBeVisible();
  const currentMonth = page.locator('details').first();
  await expect(currentMonth.locator('summary')).toBeVisible();
  await currentMonth.locator('summary').click();
  await expect(page.getByRole('table')).toBeVisible();
  await expect
    .poll(() => page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth))
    .toBe(true);
  const violations = (await new AxeBuilder({ page }).analyze()).violations.filter((item) =>
    ['serious', 'critical'].includes(item.impact || ''),
  );
  expect(violations).toEqual([]);
  await page.screenshot({ path: testInfo.outputPath('transactions.png'), fullPage: true });
  while (await page.getByRole('button', { name: 'Load more' }).isVisible()) {
    await page.getByRole('button', { name: 'Load more' }).click();
    await expect(page.getByRole('button', { name: 'Loading…' })).toHaveCount(0);
  }

  // Discover an undisputed transaction; never tie the journey to seeded IDs or names.
  const disputes = (await (await page.request.get('/api/portal/dispute?pageSize=50')).json()).data
    .items as Dispute[];
  const transactions = (
    await (await page.request.get('/api/portal/transaction?pageSize=50')).json()
  ).data.items as Transaction[];
  const transaction = transactions.find(
    (item) => !disputes.some((dispute) => dispute.transaction.transactionId === item.transactionId),
  );
  expect(transaction, 'A fresh synthetic transaction is required for this journey').toBeTruthy();
  const position = transactions.findIndex(
    (item) => item.transactionId === transaction!.transactionId,
  );
  await page
    .getByRole('button', { name: /^View .* transaction on / })
    .nth(position)
    .click();
  await expect(
    page.getByRole('heading', { name: 'Let’s look into this transaction' }),
  ).toBeVisible();
  await page.getByLabel('What went wrong?').selectOption({ index: 1 });
  await page
    .getByRole('textbox', { name: /Anything else/ })
    .fill('Synthetic browser verification. No real banking information.');
  await page.screenshot({ path: testInfo.outputPath('dispute-form.png'), fullPage: true });
  await page.getByRole('button', { name: 'Submit dispute', exact: true }).click();
  await expect(
    page.getByRole('heading', { name: 'Your dispute has been submitted' }),
  ).toBeVisible();
  await page.getByRole('link', { name: /View dispute and confirmation/ }).click();
  await expect(page.getByRole('heading', { name: 'Progress so far' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Submission confirmation' })).toBeVisible();
  await page.screenshot({ path: testInfo.outputPath('dispute-detail.png'), fullPage: true });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(
    true,
  );
  const detailViolations = (await new AxeBuilder({ page }).analyze()).violations.filter((item) =>
    ['serious', 'critical'].includes(item.impact || ''),
  );
  expect(detailViolations).toEqual([]);
  await page.getByRole('link', { name: 'Back to my disputes' }).click();
  await expect(page.getByRole('heading', { name: 'My disputes', exact: true })).toBeVisible();
  await page
    .getByRole('button', { name: 'Leave demo session', exact: true })
    .filter({ visible: true })
    .click();
  await expect(page).toHaveURL(/\/login$/);
});

test('BFF rejects cross-origin writes and unlisted routes', async ({ request }) => {
  const crossOrigin = await request.post('/api/portal/demo-session', {
    headers: { Origin: 'https://untrusted.example' },
    data: {},
  });
  expect(crossOrigin.status()).toBe(403);
  expect((await request.get('/api/portal/admin')).status()).toBe(404);
  expect((await request.get('/api/portal/transaction')).status()).toBe(401);
});

test('sign-in errors persist and transaction history stays within three months', async ({
  page,
}) => {
  await page.goto('/login');
  await page.getByLabel('Demo customer').selectOption({ index: 1 });
  await page.route('**/api/portal/demo-session', (route) =>
    route.fulfill({
      status: 503,
      contentType: 'application/json',
      body: JSON.stringify({ errors: [{ message: 'Please try again in a moment.' }] }),
    }),
  );
  await page.getByRole('button', { name: 'Continue to transactions' }).click();
  await expect(
    page.getByRole('alert').filter({ hasText: 'Please try again in a moment.' }),
  ).toContainText('Please try again in a moment.');
  await expect(page.getByRole('button', { name: 'Continue to transactions' })).toBeEnabled();
  await page.unroute('**/api/portal/demo-session');
  await page.getByRole('button', { name: 'Continue to transactions' }).focus();
  await page.keyboard.press('Enter');
  const currentMonth = page.locator('details').first();
  await expect(currentMonth.locator('summary')).toBeVisible();
  await currentMonth.locator('summary').click();
  await expect(page.getByRole('table')).toBeVisible();

  const fromDate = page.getByLabel('From date');
  const minimumDate = await fromDate.getAttribute('min');
  expect(minimumDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  await expect(fromDate).toHaveValue(minimumDate!);
  await expect(fromDate).toHaveAttribute('max', /^\d{4}-\d{2}-\d{2}$/);
  await expect(
    page.getByText('Transaction history is available for the last three months.'),
  ).toBeVisible();

  await page.goto('/?from=2020-01-01&to=2026-12-31');
  await expect(page.getByLabel('From date')).toHaveValue(minimumDate!);
  await expect(page.getByLabel('To date')).toHaveAttribute('max', /^\d{4}-\d{2}-\d{2}$/);
  await page.getByRole('link', { name: 'Clear date filters' }).click();
  await expect(page.locator('details').first().locator('summary')).toBeVisible();
});

test('transaction history can collapse and reopen month groups', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('Demo customer').selectOption({ index: 1 });
  await page.getByRole('button', { name: 'Continue to transactions' }).click();
  const monthGroup = page.locator('details').first();
  const summary = monthGroup.locator('summary');
  await expect(summary).toBeVisible();
  await expect(monthGroup).toHaveJSProperty('open', false);
  await summary.click();
  await expect(monthGroup).toHaveJSProperty('open', true);
  await expect(page.getByRole('table')).toBeVisible();
  await summary.click();
  await expect(monthGroup).toHaveJSProperty('open', false);
});

test('transaction history loads older month groups within the three-month window', async ({
  page,
}) => {
  await page.goto('/login');
  await page.getByLabel('Demo customer').selectOption({ index: 1 });
  await page.getByRole('button', { name: 'Continue to transactions' }).click();

  const monthGroups = page.locator('details');
  const loadMoreButton = page.getByRole('button', { name: 'Load more' });
  await expect(monthGroups.first()).toBeVisible();

  while ((await monthGroups.count()) < 3 && (await loadMoreButton.isVisible())) {
    await loadMoreButton.click();
    await expect(page.getByRole('button', { name: 'Loading…' })).toHaveCount(0);
  }

  expect(await monthGroups.count()).toBeGreaterThanOrEqual(3);
  const firstOlderMonth = monthGroups.nth(1);
  await expect(firstOlderMonth).toHaveJSProperty('open', false);
  await firstOlderMonth.locator('summary').click();
  await expect(firstOlderMonth).toHaveJSProperty('open', true);
  await expect(firstOlderMonth.getByRole('table')).toBeVisible();
});

test('an unknown dispute shows the not-found page', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('Demo customer').selectOption({ index: 1 });
  await page.getByRole('button', { name: 'Continue to transactions' }).click();
  await expect(page.locator('details').first().locator('summary')).toBeVisible();
  await page.goto('/disputes/00000000-0000-0000-0000-000000000000');
  await expect(page.getByText('This page isn’t available')).toBeVisible();
});
