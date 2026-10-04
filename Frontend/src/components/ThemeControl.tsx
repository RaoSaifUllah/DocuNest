import { useEffect, useState } from 'react';
import { t } from '../i18n';
import { applyTheme, readTheme, type Theme } from '../lib/theme';
import { Icon } from './Icon';

export function ThemeControl() {
  const [theme, setTheme] = useState<Theme>(readTheme);
  useEffect(() => {
    applyTheme(theme);
    const preference = window.matchMedia('(prefers-color-scheme: dark)');
    const changed = () => { if (theme === 'system') applyTheme(theme); };
    preference.addEventListener('change', changed);
    return () => preference.removeEventListener('change', changed);
  }, [theme]);
  return <div className="theme-control" role="group" aria-label={t('theme')}>
    {([
      ['system', 'monitor', 'themeSystem'],
      ['light', 'sun', 'themeLight'],
      ['dark', 'moon', 'themeDark'],
    ] as const).map(([value, icon, label]) => <button key={value} type="button" aria-label={t(label)} aria-pressed={theme === value} title={t(label)} onClick={() => setTheme(value)}><Icon name={icon} size={17} /></button>)}
  </div>;
}
