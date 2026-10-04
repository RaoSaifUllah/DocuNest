import { t } from '../i18n';
import { Icon } from '../components/Icon';

export function Overview() {
  return <div className="page overview-page">
    <div className="page-topline"><span>{t('dashboard')}</span><span className="preview-badge">{t('developmentNotice')}</span></div>
    <section className="welcome-section">
      <div className="welcome-copy"><p className="eyebrow">{t('welcomeEyebrow')}</p><h1>{t('welcomeTitle')}</h1><p className="welcome-description">{t('welcomeBody')}</p></div>
      <div className="paper-illustration" aria-hidden="true"><div className="paper rear-paper" /><div className="paper mid-paper" /><div className="paper front-paper"><span className="paper-seal"><Icon name="nest" size={30} /></span><span className="paper-line wide" /><span className="paper-line" /><span className="paper-line short" /><span className="paper-stamp"><Icon name="check" size={18} /></span></div><span className="illustration-caption">DOCUNEST</span></div>
    </section>
    <section className="start-card"><span className="start-icon"><Icon name="folder" size={32} /></span><h2>{t('getStarted')}</h2><p>{t('getStartedBody')}</p><div className="setup-notice" role="status"><Icon name="clock" size={17} /><span>{t('setupPending')}</span></div><p className="setup-detail">{t('setupPendingBody')}</p></section>
    <div className="feature-grid">
      {([
        ['shield', 'privacyTitle', 'privacyBody'],
        ['document', 'organizeTitle', 'organizeBody'],
        ['clock', 'rememberTitle', 'rememberBody'],
      ] as const).map(([icon, title, body]) => <section className="feature-card" key={title}><span className="feature-icon"><Icon name={icon} size={21} /></span><h2>{t(title)}</h2><p>{t(body)}</p></section>)}
    </div>
    <p className="foundation-note">{t('developmentNoticeBody')}</p>
  </div>;
}
