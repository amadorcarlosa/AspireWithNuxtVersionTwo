/**
 * @file auth.ts
 * @module WebApp.UI.Middleware.Auth
 * @description
 * Route middleware that ensures user is authenticated before accessing a page.
 * Equivalent to Blazor's [Authorize] attribute.
 */

export default defineNuxtRouteMiddleware(async (to) => {
  const { isAuthenticated, pending, fetchUser, login } = useAuth();

  if (pending.value) {
    await fetchUser();
  }

  if (!isAuthenticated.value) {
    // Pass the intended destination so user returns here after login
    login(to.fullPath);
    return abortNavigation();
  }
});
