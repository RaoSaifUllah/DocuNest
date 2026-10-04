import { Link } from 'react-router-dom';
import { t } from '../i18n';
import type { TranslationKey } from '../i18n/en';
import { Icon } from '../components/Icon';

export function FeaturePending({ title, description }: { title: TranslationKey; description: TranslationKey }) {
  return <div className="page"><div className="page-topline"><span>{t(title)}</span><span className="preview-badge">{t('developmentNotice')}</span></div><header className="feature-heading"><h1>{t(title)}</h1><p>{t(description)}</p></header><section className="empty-state"><span className="start-icon"><Icon name="folder" size={32} /></span><h2>{t('comingSoon')}</h2><p>{t('comingSoonBody')}</p><Link to="/" className="button primary">{t('backOverview')}<Icon name="arrow" size={17} /></Link></section></div>;
}
