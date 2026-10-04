import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { App } from './App';

function renderApp() {
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('', { status: 200 })));
  return render(<QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}><MemoryRouter><App /></MemoryRouter></QueryClientProvider>);
}

describe('application foundation', () => {
  it('communicates the real setup state without presenting synthetic document counts', async () => {
    renderApp();
    expect(screen.getByRole('heading', { name: 'Your vault starts here' })).toBeVisible();
    expect(screen.getByText('Workspace setup is not available yet')).toBeVisible();
    expect(await screen.findByText('API is responding')).toBeVisible();
    expect(screen.getByRole('link', { name: 'Skip to content' })).toHaveAttribute('href', '#main-content');
  });
  it('navigates to a transparent feature state and returns to overview', async () => {
    const user = userEvent.setup();
    renderApp();
    await user.click(screen.getByRole('link', { name: 'Documents' }));
    expect(screen.getByRole('heading', { name: 'Documents', level: 1 })).toBeVisible();
    expect(screen.getByRole('heading', { name: 'This feature is being built' })).toBeVisible();
    await user.click(screen.getByRole('link', { name: 'Back to overview' }));
    expect(screen.getByRole('heading', { name: 'Your vault starts here' })).toBeVisible();
  });
  it('offers explicit device, light, and dark choices', async () => {
    const user = userEvent.setup();
    renderApp();
    await user.click(screen.getByRole('button', { name: 'Dark theme' }));
    expect(document.documentElement.dataset.theme).toBe('dark');
    expect(screen.getByRole('button', { name: 'Dark theme' })).toHaveAttribute('aria-pressed', 'true');
    await user.click(screen.getByRole('button', { name: 'Light theme' }));
    expect(document.documentElement.dataset.theme).toBe('light');
  });
});
