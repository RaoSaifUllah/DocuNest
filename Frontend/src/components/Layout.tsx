import { useState } from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import { t } from '../i18n';
import { Icon } from './Icon';
import { ThemeControl } from './ThemeControl';
import { InstanceStatus } from './InstanceStatus';

const links = [
  { path: '/', label: 'dashboard', icon: 'grid' },
  { path: '/documents', label: 'documents', icon: 'document' },
  { path: '/reminders', label: 'reminders', icon: 'clock' },
  { path: '/shares', label: 'shares', icon: 'link' },
  { path: '/trash', label: 'trash', icon: 'trash' },
] as const;

export function Layout() {
  const [navigationOpen, setNavigationOpen] = useState(false);
  return <div className="app-layout">
    <a className="skip-link" href="#main-content">{t('skipContent')}</a>
    <header className="mobile-header"><NavLink to="/" className="brand"><span className="brand-symbol"><Icon name="nest" size={23} /></span>{t('appName')}</NavLink><button type="button" className="icon-button" aria-label={t(navigationOpen ? 'menuClose' : 'menuOpen')} aria-expanded={navigationOpen} aria-controls="sidebar" onClick={() => setNavigationOpen(!navigationOpen)}><Icon name={navigationOpen ? 'close' : 'menu'} /></button></header>
    <aside id="sidebar" className={`sidebar ${navigationOpen ? 'is-open' : ''}`}>
      <NavLink to="/" className="brand desktop-brand" onClick={() => setNavigationOpen(false)}><span className="brand-symbol"><Icon name="nest" size={23} /></span><span>{t('appName')}</span></NavLink>
      <div className="workspace-card"><span className="workspace-icon"><Icon name="folder" /></span><div><span className="small-label">{t('workspace')}</span><span className="workspace-name">{t('workspacePending')}</span></div></div>
      <nav aria-label={t('navigation')}>
        {links.map(({ path, label, icon }) => <NavLink key={path} to={path} end={path === '/'} className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`} onClick={() => setNavigationOpen(false)}><Icon name={icon} size={19} /><span>{t(label)}</span></NavLink>)}
        <div className="nav-divider" />
        <NavLink to="/settings" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`} onClick={() => setNavigationOpen(false)}><Icon name="settings" size={19} /><span>{t('settings')}</span></NavLink>
      </nav>
      <div className="sidebar-bottom"><InstanceStatus /><div className="sidebar-footer"><span>{t('selfHosted')}</span><ThemeControl /></div></div>
    </aside>
    <main id="main-content" className="main-content" tabIndex={-1}><Outlet /></main>
  </div>;
}
