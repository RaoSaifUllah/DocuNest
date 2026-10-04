import { expect, test } from '@playwright/test';

test.beforeEach(async ({ page }) => {
  await page.route('**/health/live', (route) => route.fulfill({ status: 200, contentType: 'text/plain', body: 'Healthy' }));
});

test('shell supports keyboard navigation and accurately explains setup state', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Your vault starts here' })).toBeVisible();
  await expect(page.getByText('Workspace setup is not available yet')).toBeVisible();
  await page.screenshot({ path: test.info().outputPath('overview-light.png'), fullPage: true });
  await page.keyboard.press('Tab');
  await expect(page.getByRole('link', { name: 'Skip to content' })).toBeFocused();
  await page.keyboard.press('Enter');
  await expect(page.locator('#main-content')).toBeFocused();
  if (await page.getByRole('button', { name: 'Open navigation' }).isVisible()) {
    await page.getByRole('button', { name: 'Open navigation' }).click();
  }
  await page.getByRole('link', { name: 'Documents', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Documents', exact: true })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'This feature is being built' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Back to overview' })).toBeVisible();
  await expect(page.locator('body')).toHaveJSProperty('scrollWidth', await page.locator('body').evaluate((element) => element.clientWidth));
});

test('theme is selectable and the shell makes no document requests', async ({ page }) => {
  const documentRequests: string[] = [];
  page.on('request', (request) => { if (request.url().includes('/api/v1/documents')) documentRequests.push(request.url()); });
  await page.goto('/');
  if (await page.getByRole('button', { name: 'Open navigation' }).isVisible()) {
    await page.getByRole('button', { name: 'Open navigation' }).click();
  }
  await page.getByRole('button', { name: 'Dark theme' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await page.getByRole('button', { name: 'Light theme' }).click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');
  expect(documentRequests).toEqual([]);
  expect(await page.evaluate(() => navigator.serviceWorker.getRegistrations().then((registrations) => registrations.length))).toBe(0);
});
