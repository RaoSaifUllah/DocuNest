import { defineConfig, devices } from '@playwright/test';
import { resolve } from 'node:path';

// Browser binaries remain in this project instead of a user-global directory.
process.env.PLAYWRIGHT_BROWSERS_PATH ??= resolve('.cache/ms-playwright');

export default defineConfig({
  testDir: '../tests/e2e',
  tsconfig: '../tests/e2e/tsconfig.json',
  fullyParallel: true,
  forbidOnly: Boolean(process.env.CI),
  retries: process.env.CI ? 1 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: { baseURL: 'http://127.0.0.1:5173', trace: 'retain-on-failure' },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'mobile-chromium', use: { ...devices['Pixel 7'] } },
  ],
  webServer: { command: 'npm run dev', url: 'http://127.0.0.1:5173', reuseExistingServer: !process.env.CI },
});
