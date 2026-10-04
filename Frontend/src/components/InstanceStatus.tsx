import { useQuery } from '@tanstack/react-query';
import { t } from '../i18n';
import { Icon } from './Icon';

async function checkHealth(signal: AbortSignal): Promise<boolean> {
  // Health is infrastructure, outside the versioned resource API. No credential
  // or private content is requested by the anonymous foundation shell.
  const response = await fetch('/health/live', { signal, cache: 'no-store', redirect: 'error' });
  if (!response.ok) throw new Error('Health unavailable');
  return true;
}

export function InstanceStatus() {
  const health = useQuery({ queryKey: ['instance', 'health'], queryFn: ({ signal }) => checkHealth(signal), retry: false, staleTime: 30_000 });
  return <section className="connection-card" aria-label={t('instanceStatus')}>
    <div className="connection-label">{t('instanceStatus')}</div>
    <div className="connection-status" role="status">
      <span className={`status-dot ${health.isSuccess ? 'success' : health.isError ? 'error' : 'pending'}`} />
      <span>{t(health.isPending ? 'instanceChecking' : health.isSuccess ? 'instanceReady' : 'instanceUnavailable')}</span>
    </div>
    {health.isError && <><p>{t('instanceUnavailableBody')}</p><button className="text-button" type="button" onClick={() => { void health.refetch(); }}><Icon name="arrow" size={14} />{t('retry')}</button></>}
  </section>;
}
