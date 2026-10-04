export type Theme = 'system' | 'light' | 'dark';
const preferenceKey = 'docunest.theme';

export function readTheme(): Theme {
  try {
    const stored = localStorage.getItem(preferenceKey);
    if (stored === 'light' || stored === 'dark') return stored;
  } catch { /* Storage may be disabled; device preference still works. */ }
  return 'system';
}

export function applyTheme(theme: Theme): void {
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
  document.documentElement.dataset.theme = theme === 'system' ? (prefersDark ? 'dark' : 'light') : theme;
  document.documentElement.style.colorScheme = document.documentElement.dataset.theme;
  try { localStorage.setItem(preferenceKey, theme); } catch { /* Optional preference persistence. */ }
}
