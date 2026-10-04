import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ApiError, apiRequest, tokenMemory } from './api';

describe('same-origin API boundary', () => {
  beforeEach(() => { tokenMemory.clear(); vi.unstubAllGlobals(); });
  it('keeps the JWT in memory and requests private data without browser caching', async () => {
    const fetchMock = vi.fn().mockImplementation(() => Promise.resolve(new Response(JSON.stringify({ id: 'synthetic' }), { status: 200 })));
    vi.stubGlobal('fetch', fetchMock);
    tokenMemory.set('synthetic-token');
    expect(await apiRequest('/documents')).toEqual({ id: 'synthetic' });
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/documents', expect.objectContaining({ cache: 'no-store', credentials: 'same-origin', redirect: 'error' }));
    const options = fetchMock.mock.calls[0]?.[1] as RequestInit;
    expect(new Headers(options.headers).get('Authorization')).toBe('Bearer synthetic-token');
    tokenMemory.clear();
    await apiRequest('/documents');
    expect(new Headers((fetchMock.mock.calls[1]?.[1] as RequestInit).headers).has('Authorization')).toBe(false);
    expect(localStorage.getItem('synthetic-token')).toBeNull();
  });
  it('rejects absolute and network-relative paths before a request', async () => {
    const fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);
    await expect(apiRequest('https://external.example/documents')).rejects.toThrow('same-origin');
    await expect(apiRequest('//external.example/documents')).rejects.toThrow('same-origin');
    expect(fetchMock).not.toHaveBeenCalled();
  });
  it('preserves conflict identifiers while displaying a safe localized error', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ status: 409, code: 'version_conflict', correlationId: 'synthetic-correlation', detail: 'Do not render internal diagnostics' }), { status: 409, headers: { 'Content-Type': 'application/problem+json' } })));
    const request = apiRequest('/documents/synthetic', { method: 'PATCH', body: { title: 'Updated' } });
    await expect(request).rejects.toBeInstanceOf(ApiError);
    await expect(request).rejects.toMatchObject({ status: 409, problem: { code: 'version_conflict', correlationId: 'synthetic-correlation' } });
    await expect(request).rejects.toThrow('This item changed');
  });
  it('reports a safe problem when the server returns a non-JSON failure', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('<html>proxy diagnostics</html>', { status: 503 })));
    await expect(apiRequest('/documents')).rejects.toMatchObject({ status: 503, message: 'The request could not be completed.' });
  });
});
