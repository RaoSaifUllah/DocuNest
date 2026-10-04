import { t } from '../i18n';

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
  correlationId?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails;
  readonly retryAfter: string | null;

  constructor(status: number, problem: ProblemDetails, retryAfter: string | null = null) {
    // Server-provided details remain available for audited forms but are not
    // automatically rendered: logs/exception text may contain internal data.
    super(errorMessage(status));
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
    this.retryAfter = retryAfter;
  }
}

function errorMessage(status: number): string {
  if (status === 401) return t('requestUnauthenticated');
  if (status === 403) return t('requestForbidden');
  if (status === 404) return t('requestNotFound');
  if (status === 409 || status === 412) return t('requestConflict');
  if (status === 429) return t('requestRateLimited');
  return t('requestFailed');
}

let accessToken: string | null = null;

// Access JWTs deliberately have no localStorage/sessionStorage persistence.
// Phase 2 supplies refresh/CSRF endpoints; no speculative refresh is attempted.
export const tokenMemory = {
  set(token: string): void { accessToken = token; },
  clear(): void { accessToken = null; },
};

type RequestOptions = Omit<RequestInit, 'body'> & { body?: unknown };

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  if (!path.startsWith('/') || path.startsWith('//') || path.includes('://')) {
    throw new Error('API paths must be same-origin relative paths.');
  }
  const { body, ...requestOptions } = options;
  const headers = new Headers(options.headers);
  headers.set('Accept', 'application/json, application/problem+json');
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`);
  if (body !== undefined) headers.set('Content-Type', 'application/json');
  const response = await fetch(`/api/v1${path}`, {
    ...requestOptions,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
    credentials: 'same-origin',
    cache: 'no-store',
    redirect: 'error',
  });
  if (!response.ok) {
    const contentType = response.headers.get('Content-Type') ?? '';
    let problem: ProblemDetails = { status: response.status };
    if (contentType.includes('json')) {
      try { problem = await response.json() as ProblemDetails; } catch { /* Safe generic error. */ }
    }
    throw new ApiError(response.status, problem, response.headers.get('Retry-After'));
  }
  if (response.status === 204) return undefined as T;
  return await response.json() as T;
}
