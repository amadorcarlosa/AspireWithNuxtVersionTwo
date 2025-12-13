/**
 * @file useAuthFetch.ts
 * @module WebApp.UI.Composables.AuthFetch
 * @description
 * Wrapper around `useFetch` that handles 401 responses by redirecting to login.
 * Use this for any API call to a protected endpoint.
 */

import type { UseFetchOptions } from 'nuxt/app';

export const useAuthFetch = <T>(url: string | (() => string), options: UseFetchOptions<T> = {}) => {
  const { login, isRedirecting } = useAuth();
  const route = useRoute();
  const headers = import.meta.server ? useRequestHeaders(['cookie']) : undefined;

  return useFetch<T>(url, {
    ...options,
    credentials: 'include',
    headers: {
      ...(options.headers as Record<string, string> | undefined),
      ...(headers as Record<string, string> | undefined),
    },
    redirect: 'manual',
    onResponseError({ response }) {
      // A protected endpoint might return 401 (preferred) or a 302 (legacy challenge redirect).
      if (!isRedirecting.value && (response?.status === 401 || response?.status === 302)) {
        login(route.fullPath);
      }
    },
  });
};
