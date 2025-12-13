/**
 * @file useAuthFetch.ts
 * @module WebApp.UI.Composables.AuthFetch
 * @description
 * Wrapper around `useFetch` that handles 401 responses by redirecting to login.
 * Use this for any API call to a protected endpoint.
 */

import type { UseFetchOptions } from 'nuxt/app';

export const useAuthFetch = <T>(url: string | (() => string), options: UseFetchOptions<T> = {}) => {
  const { login } = useAuth();
  const route = useRoute();

  return useFetch<T>(url, {
    ...options,
    credentials: 'include',
    onResponseError({ response }) {
      if (response?.status === 401) {
        login(route.fullPath);
      }
    },
  });
};
