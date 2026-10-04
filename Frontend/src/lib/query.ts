import { QueryClient } from '@tanstack/react-query';
import { ApiError } from './api';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      gcTime: 60_000,
      refetchOnWindowFocus: false,
      retry: (failureCount, error) => failureCount < 1 &&
        !(error instanceof ApiError && error.status < 500),
    },
    mutations: { retry: false },
  },
});

// Invoke before revealing a new workspace, and on session revocation/logout.
// There is no persistent query cache or document service-worker cache.
export async function clearScopeCache(): Promise<void> {
  await queryClient.cancelQueries();
  queryClient.clear();
}
