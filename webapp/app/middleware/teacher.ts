/**
 * @file teacher.ts
 * @module WebApp.UI.Middleware.Teacher
 * @description
 * Route middleware requiring Teacher role.
 * Equivalent to Blazor's [Authorize(Roles = "Teacher")] attribute.
 */

export default defineNuxtRouteMiddleware(async () => {
  const { pending, fetchUser, hasRole } = useAuth();

  if (pending.value) {
    await fetchUser();
  }

  if (!hasRole('Teacher')) {
    return navigateTo('/unauthorized');
  }
});
